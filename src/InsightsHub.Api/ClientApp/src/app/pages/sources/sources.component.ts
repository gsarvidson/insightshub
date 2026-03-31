import { Component, OnInit, signal, inject } from '@angular/core';
import { SourcesService } from '../../core/services/sources.service';
import { DataSource, SavedView } from '../../core/models/source.model';
import { FeedbackService } from '../../core/services/feedback.service';
import { AddFeedbackRequest } from '../../core/models/feedback.model';

const TABS = ['Data sources', 'Alerts', 'Saved views', 'Preferences'] as const;
type Tab = (typeof TABS)[number];

@Component({
  selector: 'app-sources',
  templateUrl: './sources.component.html',
  styleUrl: './sources.component.scss',
  imports: [],
})
export class SourcesComponent implements OnInit {
  private readonly sourcesService = inject(SourcesService);
  private readonly feedbackService = inject(FeedbackService);

  readonly tabs = TABS;
  activeTab  = signal<Tab>('Data sources');
  sources    = signal<DataSource[]>([]);
  savedViews = signal<SavedView[]>([]);
  loading    = signal(true);

  showModal         = signal(false);
  showCsvConfigModal = signal(false);
  csvUploading      = signal(false);
  csvProgress       = signal(0);
  csvTotal          = signal(0);
  csvDone           = signal(false);
  csvError          = signal('');
  csvPendingRows    = signal<AddFeedbackRequest[]>([]);
  csvFileName       = signal('');
  csvSource         = signal('Manual');

  readonly csvSourceOptions = [
    'App Store', 'CSAT Survey', 'NPS Survey',
    'Intercom', 'Zendesk', 'Salesforce', 'Slack', 'Manual', 'Other',
  ];

  ngOnInit() {
    this.sourcesService.getSources().subscribe({
      next: res => {
        this.sources.set(res.sources);
        this.savedViews.set(res.savedViews);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openConnectModal() {
    this.csvDone.set(false);
    this.csvError.set('');
    this.csvProgress.set(0);
    this.csvTotal.set(0);
    this.showModal.set(true);
  }

  closeModal() {
    if (this.csvUploading()) return;
    this.showModal.set(false);
  }

  closeCsvConfigModal() {
    if (this.csvUploading()) return;
    this.showCsvConfigModal.set(false);
  }

  backToConnectModal() {
    if (this.csvUploading()) return;
    this.showCsvConfigModal.set(false);
    this.csvDone.set(false);
    this.csvError.set('');
    this.showModal.set(true);
  }

  onSourceChange(event: Event) {
    this.csvSource.set((event.target as HTMLSelectElement).value);
  }

  onCsvFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      const rows = this.parseCsv(reader.result as string);
      (event.target as HTMLInputElement).value = '';
      if (!rows.length) {
        this.csvError.set('No valid rows found in CSV.');
        return;
      }
      this.csvPendingRows.set(rows);
      this.csvFileName.set(file.name);
      this.csvDone.set(false);
      this.csvError.set('');
      this.showModal.set(false);
      this.showCsvConfigModal.set(true);
    };
    reader.readAsText(file);
  }

  startCsvImport() {
    if (this.csvUploading() || this.csvDone()) return;
    const rows = this.csvPendingRows().map(r => ({ ...r, source: this.csvSource() }));
    this.csvTotal.set(rows.length);
    this.csvProgress.set(0);
    this.csvUploading.set(true);
    this.submitRows(rows, 0);
  }

  private submitRows(rows: AddFeedbackRequest[], index: number) {
    if (index >= rows.length) {
      this.csvUploading.set(false);
      this.csvDone.set(true);
      return;
    }
    this.csvProgress.set(index + 1);
    this.feedbackService.add(rows[index]).subscribe({
      next: () => this.submitRows(rows, index + 1),
      error: () => {
        this.csvUploading.set(false);
        this.csvError.set(`Upload failed on row ${index + 1}. Please try again.`);
      },
    });
  }

  private parseCsv(text: string): AddFeedbackRequest[] {
    const lines = text.split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) return [];
    const headers = this.splitCsvLine(lines[0]).map(h => h.trim().toLowerCase().replace(/[^a-z]/g, ''));
    const today = new Date().toISOString().slice(0, 10);
    return lines.slice(1).map(line => {
      const cols = this.splitCsvLine(line);
      const get = (...names: string[]) => {
        for (const n of names) {
          const i = headers.indexOf(n);
          if (i !== -1) return cols[i]?.trim() ?? '';
        }
        return '';
      };
      return {
        text:               get('text', 'feedback', 'verbatim', 'comment'),
        source:             get('source') || 'CSV Upload',
        customerType:       get('customertype', 'customer_type', 'type') || 'Consumer',
        customerIdentifier: get('customeridentifier', 'customerid', 'customer_id', 'userid', 'user_id'),
        date:               get('date') || today,
        sentiment:          get('sentiment') || 'neutral',
        oppKey:             get('oppkey', 'opp_key', 'opportunity'),
        tags:               get('tags') ? get('tags').split(';').map(t => t.trim()).filter(Boolean) : [],
        platform:           get('platform'),
        notes:              get('notes'),
      } satisfies AddFeedbackRequest;
    }).filter(r => r.text.length > 0);
  }

  private splitCsvLine(line: string): string[] {
    const result: string[] = [];
    let current = '';
    let inQuotes = false;
    for (const ch of line) {
      if (ch === '"') { inQuotes = !inQuotes; }
      else if (ch === ',' && !inQuotes) { result.push(current); current = ''; }
      else { current += ch; }
    }
    result.push(current);
    return result;
  }

  handleAction(srcId: string, action: string) {
    if (action === 'Sync now') {
      this.sourcesService.syncSource(srcId).subscribe({
        next: () => {
          this.sources.update(list =>
            list.map(s => s.id === srcId ? { ...s, lastSynced: 'just now' } : s)
          );
        },
      });
    } else if (action === 'Disconnect') {
      this.sourcesService.disconnectSource(srcId).subscribe({
        next: () => {
          this.sources.update(list => list.filter(s => s.id !== srcId));
        },
      });
    }
  }
}

import { Component, signal, inject, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FeedbackService } from '../../core/services/feedback.service';

interface CsvPreviewRow {
  vertical: string;
  userType: string;
  date: string;
  feedback: string;
  whoRaisedIt: string;
  impact: string;
  submittedBy: string;
}

@Component({
  selector: 'app-add-feedback',
  templateUrl: './add-feedback.component.html',
  styleUrl: './add-feedback.component.scss',
  imports: [ReactiveFormsModule],
})
export class AddFeedbackComponent {
  private readonly fb = inject(FormBuilder);
  private readonly feedbackService = inject(FeedbackService);
  private readonly router = inject(Router);

  activeTab = signal<'manual' | 'csv'>('manual');

  // Manual form state
  submitting = signal(false);
  success = signal(false);
  error = signal('');

  // CSV import state
  csvFile = signal<File | null>(null);
  csvPreview = signal<CsvPreviewRow[]>([]);
  csvParseError = signal('');
  csvSource = signal('Manual');
  csvImporting = signal(false);
  csvResult = signal<{ imported: number; skipped: number } | null>(null);
  csvImportError = signal('');

  private readonly options = toSignal(this.feedbackService.getOptions());

  readonly sources = computed(() => this.options()?.sources ?? []);
  readonly customerTypes = computed(() => this.options()?.customerTypes ?? []);
  readonly verticals = computed(() => this.options()?.verticals ?? []);
  readonly opportunityOptions = computed(() => [
    { value: '', label: '— None —' },
    ...(this.options()?.opportunities ?? []).map(o => ({ value: o.id, label: o.title })),
  ]);
  readonly sentiments = ['positive', 'neutral', 'negative'] as const;

  form = this.fb.group({
    text: ['', [Validators.required, Validators.minLength(10)]],
    source: ['', Validators.required],
    customerType: ['Consumer'],
    vertical: [''],
    customerId: [''],
    date: [new Date().toISOString().slice(0, 10)],
    sentiment: ['neutral'],
    opportunityId: [''],
    tags: [''],
    platform: [''],
    notes: [''],
  });

  readonly sentimentLabels: Record<string, string> = {
    negative: '😤 Negative',
    neutral:  '😐 Neutral',
    positive: '😊 Positive',
  };

  sentimentLabel(s: string): string {
    return this.sentimentLabels[s] ?? s;
  }

  setSentiment(value: string) {
    this.form.patchValue({ sentiment: value });
  }

  clearForm() {
    this.form.reset({
      customerType: 'Consumer',
      vertical: '',
      sentiment: 'neutral',
      date: new Date().toISOString().slice(0, 10),
    });
  }

  onCsvFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.csvFile.set(file);
    this.csvPreview.set([]);
    this.csvParseError.set('');
    this.csvResult.set(null);
    this.csvImportError.set('');

    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      const rows = this.parseCsvPreview(text);
      if (rows === null) {
        this.csvParseError.set('Could not parse CSV — check the file has the expected headers.');
      } else if (rows.length === 0) {
        this.csvParseError.set('The CSV has no data rows.');
      } else {
        this.csvPreview.set(rows);
      }
    };
    reader.readAsText(file);
  }

  private parseCsvPreview(csv: string): CsvPreviewRow[] | null {
    const lines = csv.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length < 2) return null;

    const headers = this.splitCsvLine(lines[0]).map(h => h.toLowerCase().trim());
    const idx = (name: string) => headers.indexOf(name.toLowerCase());

    const iVertical   = idx('vertical');
    const iUserType   = idx('user type');
    const iDate       = idx('created date');
    const iFeedback   = idx('feedback');
    const iWho        = idx('who raised it');
    const iImpact     = idx('impact');
    const iSubmitter  = idx('submitted by');

    if (iFeedback === -1) return null; // minimum required column

    const rows: CsvPreviewRow[] = [];
    for (let i = 1; i < lines.length; i++) {
      const vals = this.splitCsvLine(lines[i]);
      const get = (col: number) => (col >= 0 && col < vals.length) ? vals[col].trim() : '';
      const feedback = get(iFeedback);
      if (!feedback) continue;
      rows.push({
        vertical:    get(iVertical),
        userType:    get(iUserType),
        date:        get(iDate),
        feedback,
        whoRaisedIt: get(iWho),
        impact:      get(iImpact),
        submittedBy: get(iSubmitter),
      });
    }
    return rows;
  }

  private splitCsvLine(line: string): string[] {
    const fields: string[] = [];
    let current = '';
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const c = line[i];
      if (c === '"') {
        if (inQuotes && line[i + 1] === '"') { current += '"'; i++; }
        else { inQuotes = !inQuotes; }
      } else if (c === ',' && !inQuotes) {
        fields.push(current);
        current = '';
      } else {
        current += c;
      }
    }
    fields.push(current);
    return fields;
  }

  importCsv() {
    const file = this.csvFile();
    if (!file || this.csvImporting()) return;
    this.csvImporting.set(true);
    this.csvImportError.set('');
    this.csvResult.set(null);

    this.feedbackService.importCsv(file, this.csvSource()).subscribe({
      next: (result) => {
        this.csvImporting.set(false);
        this.csvResult.set(result);
        this.csvFile.set(null);
        this.csvPreview.set([]);
      },
      error: () => {
        this.csvImporting.set(false);
        this.csvImportError.set('Import failed. Please check the file and try again.');
      },
    });
  }

  onSubmit() {
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);
    this.error.set('');

    const v = this.form.value;
    const payload = {
      text: v.text ?? '',
      source: v.source ?? '',
      customerType: v.customerType ?? '',
      customerIdentifier: v.customerId ?? '',
      date: v.date ?? '',
      sentiment: v.sentiment ?? 'neutral',
      oppKey: v.opportunityId ?? '',
      tags: (v.tags ?? '').split(',').map((t: string) => t.trim()).filter(Boolean),
      platform: v.platform ?? '',
      notes: v.notes ?? '',
      team: v.vertical ?? '',
    };

    this.feedbackService.add(payload).subscribe({
      next: () => {
        this.submitting.set(false);
        this.success.set(true);
        this.form.reset({
          customerType: 'Consumer',
          vertical: '',
          sentiment: 'neutral',
          date: new Date().toISOString().slice(0, 10),
        });
        setTimeout(() => this.success.set(false), 4000);
      },
      error: () => {
        this.submitting.set(false);
        this.error.set('Failed to save feedback. Please try again.');
      },
    });
  }
}

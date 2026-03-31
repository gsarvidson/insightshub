import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FeedbackService } from '../../core/services/feedback.service';
import { OpportunityService } from '../../core/services/opportunity.service';
import { FeedbackItem, FeedbackPage } from '../../core/models/feedback.model';
import { Opportunity } from '../../core/models/opportunity.model';
import { D3LineChartComponent, LineSeries } from '../../shared/components/d3-line-chart/d3-line-chart.component';

const TAG_COLORS: Record<string, string> = {
  'Checkout & Payments': '#E24B4A',
  'Search Relevance':    '#EF9F27',
  'Watchlist':           '#378ADD',
  'Photo Upload':        '#1D9E75',
  'Buyer Messaging':     '#9B59B6',
  'Seller Fees':         '#F0A500',
  'Performance':         '#20B2AA',
  'Onboarding':          '#FF6347',
  'Bug':                 '#CC3333',
  'Feature Request':     '#5599DD',
  'UX Issue':            '#AA44BB',
  'High Priority':       '#DD5500',
};
const FALLBACK_COLORS = ['#E24B4A','#EF9F27','#378ADD','#1D9E75','#9B59B6','#F0A500','#20B2AA','#FF6347'];

@Component({
  selector: 'app-feedback',
  imports: [FormsModule, RouterLink, D3LineChartComponent],
  templateUrl: './feedback.component.html',
  styleUrl: './feedback.component.scss',
})
export class FeedbackComponent implements OnInit, OnDestroy {
  private readonly feedbackService = inject(FeedbackService);
  private readonly opportunityService = inject(OpportunityService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  feedbackPage  = signal<FeedbackPage | null>(null);
  loading       = signal(true);
  selectedId    = signal<string | null>(null);
  drawerOpen    = signal(false);
  page          = signal(1);

  // Filters
  sourceFilter    = signal('');
  sentimentFilter = signal('');
  themeFilter     = signal('');
  dateFilter      = signal('10w');
  searchText      = signal('');
  oppFilter       = signal('');

  // Tag autocomplete
  tagInputOpen    = signal(false);
  tagInputValue   = signal('');
  tagSuggestions  = signal<{ name: string; color: string }[]>([]);
  tagSearching    = signal(false);
  private readonly tagSearch$ = new Subject<string>();

  // Link opportunity modal
  linkModalOpen   = signal(false);
  oppListLoading  = signal(false);
  allOpps         = signal<Opportunity[]>([]);
  oppSearch       = signal('');
  selectedOppId   = signal<string | null>(null);
  linkSaving      = signal(false);

  readonly filteredOpps = computed(() => {
    const q = this.oppSearch().toLowerCase();
    return q
      ? this.allOpps().filter(o => o.title.toLowerCase().includes(q) || o.sub.toLowerCase().includes(q))
      : this.allOpps();
  });

  // Chart
  themeSeries = signal<LineSeries[]>([]);
  chartLabels: string[] = [];

  readonly activeSeries = computed(() => this.themeSeries().filter(s => s.active !== false));

  readonly selectedItem = computed(() =>
    this.feedbackPage()?.items.find(f => f.id === this.selectedId()) ?? null
  );

  ngOnInit() {
    const qp = this.route.snapshot.queryParamMap;
    const page = parseInt(qp.get('page') ?? '1', 10);
    if (page > 1) this.page.set(page);
    const source = qp.get('source');    if (source)    this.sourceFilter.set(source);
    const sentiment = qp.get('sentiment'); if (sentiment) this.sentimentFilter.set(sentiment);
    const theme = qp.get('theme');      if (theme)     this.themeFilter.set(theme);
    const search = qp.get('search');    if (search)    this.searchText.set(search);
    const opp = qp.get('opp');          if (opp)       this.oppFilter.set(opp);
    this.loadPage();
    this.loadTrends();
    this.tagSearch$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => {
        if (q.length < 3) { this.tagSuggestions.set([]); return of([]); }
        this.tagSearching.set(true);
        return this.feedbackService.searchTags(q);
      }),
    ).subscribe({ next: results => { this.tagSuggestions.set(results); this.tagSearching.set(false); } });
  }

  ngOnDestroy() {
    this.tagSearch$.complete();
  }

  loadTrends() {
    this.feedbackService.getTrends().subscribe({
      next: result => {
        this.chartLabels = result.labels;
        this.themeSeries.set(result.series.map((s, i) => ({
          label: s.label,
          color: TAG_COLORS[s.label] ?? FALLBACK_COLORS[i % FALLBACK_COLORS.length],
          data: s.data,
          active: i < 3,
        })));
      },
    });
  }

  private syncQueryParams() {
    const params: Record<string, string | null> = {
      page:      this.page() > 1 ? String(this.page()) : null,
      source:    this.sourceFilter()    || null,
      sentiment: this.sentimentFilter() || null,
      theme:     this.themeFilter()     || null,
      search:    this.searchText()      || null,
      opp:       this.oppFilter()       || null,
    };
    this.router.navigate([], { relativeTo: this.route, queryParams: params, replaceUrl: true });
  }

  loadPage() {
    this.loading.set(true);
    this.syncQueryParams();
    this.feedbackService.getPage({
      page: this.page(),
      source: this.sourceFilter(),
      sentiment: this.sentimentFilter(),
      theme: this.themeFilter(),
      search: this.searchText(),
      opp: this.oppFilter() || undefined,
    }).subscribe({
      next: p => { this.feedbackPage.set(p); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setThemeFilter(v: string) {
    this.themeFilter.set(v);
    this.applyFilters();
  }

  setDateFilter(v: string) {
    this.dateFilter.set(v);
  }

  applyFilters() {
    this.page.set(1);
    this.loadPage();
  }

  clearFilters() {
    this.sourceFilter.set('');
    this.sentimentFilter.set('');
    this.themeFilter.set('');
    this.dateFilter.set('10w');
    this.searchText.set('');
    this.oppFilter.set('');
    this.page.set(1);
    this.loadPage();
  }

  prevPage() {
    if (this.page() > 1) { this.page.update(p => p - 1); this.loadPage(); }
  }

  nextPage() {
    const p = this.feedbackPage();
    if (p && this.page() * p.pageSize < p.total) {
      this.page.update(n => n + 1);
      this.loadPage();
    }
  }

  selectRow(id: string) {
    this.selectedId.set(id);
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    this.drawerOpen.set(false);
    setTimeout(() => this.selectedId.set(null), 300);
  }

  toggleTheme(label: string) {
    this.themeSeries.update(series =>
      series.map(s => s.label === label ? { ...s, active: !s.active } : s)
    );
  }

  sentClass(s: string) {
    return `sentiment sent-${s}`;
  }

  srcClass(s: string) {
    return `source-badge src-${s}`;
  }

  sentLabel(s: string) {
    return s === 'neg' ? 'Negative' : s === 'pos' ? 'Positive' : 'Neutral';
  }

  get pageInfo() {
    const p = this.feedbackPage();
    if (!p) return '';
    const start = (p.page - 1) * p.pageSize + 1;
    const end = Math.min(p.page * p.pageSize, p.total);
    return `${start}–${end} of ${p.total}`;
  }

  get hasPrev() { return this.page() > 1; }
  get hasNext() {
    const p = this.feedbackPage();
    return p ? this.page() * p.pageSize < p.total : false;
  }

  openTagInput() {
    this.tagInputValue.set('');
    this.tagSuggestions.set([]);
    this.tagInputOpen.set(true);
  }

  closeTagInput() {
    this.tagInputOpen.set(false);
    this.tagSuggestions.set([]);
  }

  onTagInputChange(value: string) {
    this.tagInputValue.set(value);
    this.tagSearch$.next(value);
  }

  pickTag(tag: { name: string; color: string }) {
    const feedbackId = this.selectedId();
    if (!feedbackId) return;

    // Skip if already tagged
    const item = this.selectedItem();
    if (item?.themes.some(t => t.name === tag.name)) { this.closeTagInput(); return; }

    this.feedbackService.addTag(feedbackId, tag.name).subscribe({
      next: added => {
        this.feedbackPage.update(p => {
          if (!p) return p;
          return {
            ...p,
            items: p.items.map(fi =>
              fi.id === feedbackId
                ? { ...fi, themes: [...fi.themes, { name: added.name, color: added.color }] }
                : fi
            ),
          };
        });
        this.closeTagInput();
      },
    });
  }

  openLinkModal() {
    this.selectedOppId.set(null);
    this.oppSearch.set('');
    this.linkModalOpen.set(true);
    if (this.allOpps().length === 0) {
      this.oppListLoading.set(true);
      this.opportunityService.getAll().subscribe({
        next: list => { this.allOpps.set(list); this.oppListLoading.set(false); },
        error: ()  => this.oppListLoading.set(false),
      });
    }
  }

  closeLinkModal() {
    this.linkModalOpen.set(false);
  }

  confirmLink() {
    const feedbackId = this.selectedId();
    const oppId      = this.selectedOppId();
    if (!feedbackId || !oppId) return;

    const opp = this.allOpps().find(o => o.id === oppId);
    this.linkSaving.set(true);
    this.feedbackService.linkOpportunity(feedbackId, oppId).subscribe({
      next: () => {
        // Update item in local page data
        this.feedbackPage.update(p => {
          if (!p) return p;
          return {
            ...p,
            items: p.items.map(item =>
              item.id === feedbackId
                ? { ...item, oppKey: oppId, opp: opp?.title ?? oppId }
                : item
            ),
          };
        });
        this.linkSaving.set(false);
        this.closeLinkModal();
      },
      error: () => this.linkSaving.set(false),
    });
  }
}

import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OpportunityService } from '../../core/services/opportunity.service';
import { Opportunity } from '../../core/models/opportunity.model';

const STATUS_FILTERS = ['All', 'Urgent', 'Under review', 'On roadmap', 'Backlog', 'Done'] as const;
type StatusFilter = (typeof STATUS_FILTERS)[number];

const STATUS_CLASS: Record<string, string> = {
  'Urgent':       's-urgent',
  'Under review': 's-review',
  'On roadmap':   's-road',
  'Backlog':      's-backlog',
  'Done':         's-done',
};

@Component({
  selector: 'app-opportunities',
  imports: [RouterLink],
  templateUrl: './opportunities.component.html',
  styleUrl: './opportunities.component.scss',
})
export class OpportunitiesComponent implements OnInit {
  private readonly oppService = inject(OpportunityService);

  readonly filters = STATUS_FILTERS;
  activeFilter = signal<StatusFilter>('All');
  allOpps      = signal<Opportunity[]>([]);
  selectedId   = signal<string | null>(null);
  detailOpen   = signal(false);
  statusDropOpen = signal(false);
  loading      = signal(true);

  readonly filteredOpps = computed(() => {
    const filter = this.activeFilter();
    const opps = this.allOpps();
    return filter === 'All' ? opps : opps.filter(o => o.status === filter);
  });

  readonly selectedOpp = computed(() =>
    this.allOpps().find(o => o.id === this.selectedId()) ?? null
  );

  readonly availableStatuses = STATUS_FILTERS.filter(s => s !== 'All');

  ngOnInit() {
    this.oppService.getAll().subscribe({
      next: opps => { this.allOpps.set(opps); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setFilter(f: StatusFilter) {
    this.activeFilter.set(f);
  }

  selectOpp(id: string) {
    this.selectedId.set(id);
    this.detailOpen.set(true);
  }

  closeDetail() {
    this.detailOpen.set(false);
    setTimeout(() => this.selectedId.set(null), 300);
  }

  statusClass(status: string) {
    return STATUS_CLASS[status] ?? 's-backlog';
  }

  updateStatus(newStatus: string) {
    const id = this.selectedId();
    if (!id) return;
    this.statusDropOpen.set(false);
    this.oppService.updateStatus(id, newStatus).subscribe(() => {
      this.allOpps.update(opps =>
        opps.map(o => o.id === id ? { ...o, status: newStatus } : o)
      );
    });
  }

  trendChipStyle(trend: string) {
    if (trend.includes('spike') || trend.includes('Growing'))
      return { background: '#FCEBEB', color: '#A32D2D' };
    if (trend.startsWith('↑'))
      return { background: '#FAEEDA', color: '#854F0B' };
    return { background: '#F4F4FC', color: '#5a5751' };
  }
}

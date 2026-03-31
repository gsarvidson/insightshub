import { Component, OnInit, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummary } from '../../core/models/dashboard.model';
import { MetricCardComponent } from '../../shared/components/metric-card/metric-card.component';
import { AiSummaryBoxComponent } from '../../shared/components/ai-summary-box/ai-summary-box.component';
import { D3BarChartComponent } from '../../shared/components/d3-bar-chart/d3-bar-chart.component';
import { D3DonutChartComponent } from '../../shared/components/d3-donut-chart/d3-donut-chart.component';

@Component({
  selector: 'app-dashboard',
  imports: [MetricCardComponent, AiSummaryBoxComponent, D3BarChartComponent, D3DonutChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly router = inject(Router);

  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);

  ngOnInit() {
    this.dashboardService.getSummary().subscribe({
      next: s => { this.summary.set(s); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  navigate(target: string) {
    this.router.navigate([`/${target}`]);
  }

  get volumeBarData() {
    const s = this.summary();
    if (!s) return [];
    return s.volumeData.map((v, i) => ({
      label: s.volumeLabels[i],
      value: v,
      color: '#4854D3',
    }));
  }

  get donutData() {
    return (this.summary()?.sourceBreakdown ?? []).map(src => ({
      label: src.name,
      value: src.count,
      color: src.color,
    }));
  }

  get metrics() {
    return this.summary()?.metrics;
  }
}

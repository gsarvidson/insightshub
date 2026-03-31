import { Component, input } from '@angular/core';

@Component({
  selector: 'app-metric-card',
  template: `
    <div class="metric">
      <div class="metric-label">{{ label() }}</div>
      <div class="metric-value">{{ value() }}</div>
      @if (delta()) {
        <div class="metric-delta" [class.delta-up]="deltaClass() === 'up'" [class.delta-down]="deltaClass() === 'down'">
          {{ delta() }}
        </div>
      }
      @if (sub()) {
        <div class="metric-sub">{{ sub() }}</div>
      }
    </div>
  `,
  styles: [`
    .metric {
      background: var(--color-background-secondary);
      border-radius: var(--border-radius-md);
      padding: 0.875rem 1rem;
      border-left: 3px solid var(--tm-blurple);
      transition: box-shadow 0.15s;

      &:hover { box-shadow: 0 2px 8px rgba(72,84,211,0.12); }
    }
    .metric-label { font-size: 12px; color: var(--color-text-secondary); margin-bottom: 6px; }
    .metric-value { font-family: var(--font-display); font-weight: 700; font-size: 22px; color: var(--color-text-primary); }
    .metric-delta { font-size: 12px; margin-top: 4px; }
    .delta-up   { color: var(--color-text-danger); }
    .delta-down { color: var(--color-text-success); }
    .metric-sub { font-size: 11px; color: var(--color-text-tertiary); margin-top: 3px; }
  `],
})
export class MetricCardComponent {
  label    = input.required<string>();
  value    = input.required<string>();
  delta    = input<string>();
  deltaClass = input<'up' | 'down'>('up');
  sub      = input<string>();
}

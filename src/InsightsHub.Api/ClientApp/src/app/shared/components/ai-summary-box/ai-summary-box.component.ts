import { Component, input } from '@angular/core';

@Component({
  selector: 'app-ai-summary-box',
  template: `
    <div class="ai-box">
      <div class="ai-label">{{ label() }}</div>
      <div class="ai-text" [innerHTML]="text()"></div>
    </div>
  `,
  styles: [`
    .ai-box {
      background: linear-gradient(135deg, var(--tm-blurple-dark) 0%, var(--tm-blurple) 100%);
      border: none;
      border-radius: var(--border-radius-md);
      padding: 0.875rem 1rem;
      margin-bottom: 1.5rem;
    }
    .ai-label {
      font-size: 11px;
      font-family: var(--font-display);
      font-weight: 500;
      color: var(--tm-kowhai);
      margin-bottom: 6px;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .ai-text {
      font-size: 12px;
      color: rgba(255, 255, 255, 0.9);
      line-height: 1.6;
    }
    ::ng-deep .ai-text strong {
      color: var(--tm-kowhai);
    }
  `],
})
export class AiSummaryBoxComponent {
  label = input<string>('AI summary');
  text  = input.required<string>();
}

import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'opportunities',
    loadComponent: () =>
      import('./pages/opportunities/opportunities.component').then(m => m.OpportunitiesComponent),
  },
  {
    path: 'feedback',
    loadComponent: () =>
      import('./pages/feedback/feedback.component').then(m => m.FeedbackComponent),
  },
  {
    path: 'ai-assistant',
    loadComponent: () =>
      import('./pages/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent),
  },
  {
    path: 'sources',
    loadComponent: () =>
      import('./pages/sources/sources.component').then(m => m.SourcesComponent),
  },
  {
    path: 'add-feedback',
    loadComponent: () =>
      import('./pages/add-feedback/add-feedback.component').then(m => m.AddFeedbackComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];

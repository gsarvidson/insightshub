import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly primaryNav: NavItem[] = [
    { label: 'Dashboard', route: 'dashboard' },
    { label: 'Opportunities', route: 'opportunities' },
    { label: 'Feedback', route: 'feedback' },
    { label: 'AI Assistant', route: 'ai-assistant' },
  ];

  readonly secondaryNav: NavItem[] = [
    { label: 'Add feedback', route: 'add-feedback' },
    { label: 'Sources', route: 'sources' },
  ];
}

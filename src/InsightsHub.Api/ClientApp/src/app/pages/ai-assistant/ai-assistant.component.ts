import { Component, signal, inject, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AiService } from '../../core/services/ai.service';
import { ChatMessage } from '../../core/models/chat.model';

interface UiMessage {
  role: 'user' | 'assistant';
  content: string;
  isLoading?: boolean;
}

const INITIAL_MESSAGES: UiMessage[] = [
  {
    role: 'assistant',
    content: 'Hello! I can answer questions about your product feedback, trends, and opportunities. What would you like to know?',
  },
];

const SUGGESTED_QUESTIONS = [
  'What are the top issues this week?',
  'What do sellers need most?',
  'Any new trends emerging?',
  'Compare buyer vs seller pain points',
];

@Component({
  selector: 'app-ai-assistant',
  imports: [FormsModule],
  templateUrl: './ai-assistant.component.html',
  styleUrl: './ai-assistant.component.scss',
})
export class AiAssistantComponent implements AfterViewChecked {
  private readonly aiService = inject(AiService);
  private readonly router = inject(Router);

  @ViewChild('messagesEl') messagesEl!: ElementRef<HTMLDivElement>;

  messages     = signal<UiMessage[]>(INITIAL_MESSAGES);
  inputText    = signal('');
  isLoading    = signal(false);
  suggestions  = SUGGESTED_QUESTIONS;
  private shouldScroll = false;

  ngAfterViewChecked() {
    if (this.shouldScroll) {
      const el = this.messagesEl?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
      this.shouldScroll = false;
    }
  }

  fillSuggestion(q: string) {
    this.inputText.set(q);
  }

  send() {
    const text = this.inputText().trim();
    if (!text || this.isLoading()) return;

    this.messages.update(m => [...m, { role: 'user', content: text }]);
    this.inputText.set('');
    this.isLoading.set(true);
    this.shouldScroll = true;

    const history: ChatMessage[] = this.messages()
      .filter(m => !m.isLoading)
      .map(m => ({ role: m.role, content: m.content }));

    this.aiService.chat(history).subscribe({
      next: res => {
        this.isLoading.set(false);
        if (res.success) {
          this.messages.update(m => [...m, { role: 'assistant', content: res.content }]);
        } else {
          this.messages.update(m => [...m, {
            role: 'assistant',
            content: `Error: ${res.error ?? 'Unknown error. Is the Anthropic API key configured in appsettings?'}`,
          }]);
        }
        this.shouldScroll = true;
      },
      error: () => {
        this.isLoading.set(false);
        this.messages.update(m => [...m, {
          role: 'assistant',
          content: 'Failed to reach the AI service. Check that the .NET API is running.',
        }]);
        this.shouldScroll = true;
      },
    });
  }

  handleKey(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text);
  }

  navigate(target: string) {
    this.router.navigate([`/${target}`]);
  }

  navigateTo(path: string) {
    this.router.navigate([path]);
  }

  formatContent(text: string) {
    return text
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\n\n/g, '</p><p>')
      .replace(/\n(\d+)\.\s/g, '</p><p>$1. ')
      .replace(/\n/g, '<br>');
  }
}

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ChatMessage, ChatResponse } from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly api = inject(ApiService);

  chat(messages: ChatMessage[]): Observable<ChatResponse> {
    return this.api.post<ChatResponse>('/ai/chat', { messages });
  }
}

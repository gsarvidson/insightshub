import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AddFeedbackRequest, FeedbackOptions, FeedbackPage, FeedbackTrendsResult } from '../models/feedback.model';

export interface FeedbackFilters {
  page?: number;
  pageSize?: number;
  source?: string;
  sentiment?: string;
  theme?: string;
  search?: string;
}

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly api = inject(ApiService);

  getPage(filters: FeedbackFilters = {}): Observable<FeedbackPage> {
    const params: Record<string, string | number | boolean> = {
      page: filters.page ?? 1,
      pageSize: filters.pageSize ?? 10,
    };
    if (filters.source)    params['source'] = filters.source;
    if (filters.sentiment) params['sentiment'] = filters.sentiment;
    if (filters.theme)     params['theme'] = filters.theme;
    if (filters.search)    params['search'] = filters.search;
    return this.api.get<FeedbackPage>('/feedback', params);
  }

  getTrends(weeks = 10): Observable<FeedbackTrendsResult> {
    return this.api.get<FeedbackTrendsResult>('/feedback/trends', { weeks });
  }

  getOptions(): Observable<FeedbackOptions> {
    return this.api.get<FeedbackOptions>('/feedback/options');
  }

  add(req: AddFeedbackRequest): Observable<{ success: boolean }> {
    return this.api.post<{ success: boolean }>('/feedback', req);
  }

  importCsv(file: File, source: string): Observable<{ imported: number; skipped: number }> {
    const fd = new FormData();
    fd.append('file', file);
    fd.append('source', source);
    return this.api.postFormData<{ imported: number; skipped: number }>('/feedback/import', fd);
  }

  linkOpportunity(feedbackId: string, oppId: string): Observable<{ success: boolean }> {
    return this.api.patch<{ success: boolean }>(`/feedback/${feedbackId}/opportunity`, { oppId });
  }

  searchTags(q: string): Observable<{ name: string; color: string }[]> {
    return this.api.get<{ name: string; color: string }[]>('/tags/search', { q });
  }

  addTag(feedbackId: string, name: string): Observable<{ name: string; color: string }> {
    return this.api.post<{ name: string; color: string }>(`/feedback/${feedbackId}/tags`, { name });
  }
}

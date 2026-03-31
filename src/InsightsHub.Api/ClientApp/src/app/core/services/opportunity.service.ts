import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Opportunity } from '../models/opportunity.model';

@Injectable({ providedIn: 'root' })
export class OpportunityService {
  private readonly api = inject(ApiService);

  getAll(status?: string): Observable<Opportunity[]> {
    return this.api.get<Opportunity[]>('/opportunities', status ? { status } : undefined);
  }

  getById(id: string): Observable<Opportunity> {
    return this.api.get<Opportunity>(`/opportunities/${id}`);
  }

  updateStatus(id: string, status: string): Observable<{ id: string; status: string }> {
    return this.api.patch<{ id: string; status: string }>(`/opportunities/${id}/status`, { status });
  }

  create(req: { title: string; sub: string; status?: string }): Observable<Opportunity> {
    return this.api.post<Opportunity>('/opportunities', req);
  }
}

import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { DataSource, DataSourceDto, SavedView, SourcesResponse } from '../models/source.model';

const SOURCE_STYLES: Record<string, { iconBg: string; iconColor: string }> = {
  'app store':    { iconBg: '#e0f2fe', iconColor: '#0284c7' },
  'csat survey':  { iconBg: '#f0fdf4', iconColor: '#16a34a' },
  'nps survey':   { iconBg: '#fefce8', iconColor: '#ca8a04' },
  'intercom':     { iconBg: '#f0f9ff', iconColor: '#0369a1' },
  'zendesk':      { iconBg: '#fdf4ff', iconColor: '#9333ea' },
  'salesforce':   { iconBg: '#eff6ff', iconColor: '#2563eb' },
  'slack':        { iconBg: '#fff7ed', iconColor: '#ea580c' },
  'manual':       { iconBg: '#f8fafc', iconColor: '#64748b' },
};

const DEFAULT_STYLE = { iconBg: '#f1f5f9', iconColor: '#475569' };

function toIconLabel(name: string): string {
  const words = name.trim().split(/\s+/);
  return words.length >= 2
    ? (words[0][0] + words[1][0]).toUpperCase()
    : name.slice(0, 2).toUpperCase();
}

function toStatusColor(status: string): string {
  return status.toLowerCase() === 'active' ? '#22c55e' : '#f59e0b';
}

function toActions(status: string): string[] {
  return status.toLowerCase() === 'active' ? ['Sync now', 'Disconnect'] : ['Connect'];
}

function toDataSource(dto: DataSourceDto): DataSource {
  const style = SOURCE_STYLES[dto.name.toLowerCase()] ?? DEFAULT_STYLE;
  return {
    ...dto,
    iconLabel: toIconLabel(dto.name),
    iconBg: style.iconBg,
    iconColor: style.iconColor,
    statusColor: toStatusColor(dto.status),
    actions: toActions(dto.status),
  };
}

export interface MappedSourcesResponse {
  sources: DataSource[];
  savedViews: SavedView[];
}

@Injectable({ providedIn: 'root' })
export class SourcesService {
  private readonly api = inject(ApiService);

  getSources(): Observable<MappedSourcesResponse> {
    return this.api.get<SourcesResponse>('/sources').pipe(
      map(res => ({
        sources: res.sources.map(toDataSource),
        savedViews: res.savedViews,
      }))
    );
  }

  syncSource(id: string): Observable<void> {
    return this.api.post<void>(`/sources/${id}/sync`, {});
  }

  disconnectSource(id: string): Observable<void> {
    return this.api.delete<void>(`/sources/${id}`);
  }
}

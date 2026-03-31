export interface DataSourceDto {
  id: string;
  name: string;
  lastSynced: string;
  description: string;
  status: string;
}

export interface DataSource extends DataSourceDto {
  iconLabel: string;
  iconBg: string;
  iconColor: string;
  statusColor: string;
  actions: string[];
}

export interface SavedView {
  name: string;
  meta: string;
}

export interface SourcesResponse {
  sources: DataSourceDto[];
  savedViews: SavedView[];
}

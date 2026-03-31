export interface SourceCount {
  name: string;
  count: number;
}

export interface Tag {
  name: string;
  color: string;
}

export interface Opportunity {
  id: string;
  title: string;
  sub: string;
  status: string;
  mentions: number;
  trend: string;
  trendColor: string;
  scorePercent: number;
  sources: SourceCount[];
  tags: Tag[];
  trendBars?: number[];
  teams: string[];
  color: string;
  aiNotes?: string;
}

export interface UpdateStatusRequest {
  status: string;
}

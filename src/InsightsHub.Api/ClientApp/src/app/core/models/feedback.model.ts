export interface FeedbackItem {
  id: string;
  text: string;
  meta: string;
  src: string;
  srcLabel: string;
  date: string;
  sentiment: string;
  sentimentColor: string;
  themes: { name: string; color: string }[];
  opp: string;
  oppKey: string;
  userType: string;
  platform: string;
  aiNote: string;
  teams: string[];
}

export interface FeedbackPage {
  items: FeedbackItem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface TagWeeklyData { label: string; data: number[]; }
export interface FeedbackTrendsResult { labels: string[]; series: TagWeeklyData[]; }

export interface FeedbackOptionItem {
  id: string;
  title: string;
}

export interface FeedbackOptions {
  sources: string[];
  customerTypes: string[];
  verticals: string[];
  opportunities: FeedbackOptionItem[];
}

export interface AddFeedbackRequest {
  text: string;
  source: string;
  customerType?: string;
  customerIdentifier?: string;
  date?: string;
  sentiment?: string;
  oppKey?: string;
  tags?: string[];
  platform?: string;
  notes?: string;
  team?: string;
}

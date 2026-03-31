export interface DashboardMetrics {
  totalFeedback: number;
  totalFeedbackDelta: string;
  openOpportunities: number;
  openOpportunitiesDelta: string;
  trendingThemes: number;
  trendingThemesDelta: string;
  avgSentimentPct: number;
  avgSentimentDelta: string;
}

export interface TrendingTheme {
  label: string;
  count: number;
  trendChip?: string;
  barColor: string;
  barWidthPct: number;
  isUrgent: boolean;
}

export interface DashboardAlert {
  text: string;
  time: string;
  color: string;
  navTarget?: string;
}

export interface SourceBreakdown {
  name: string;
  count: number;
  pct: number;
  color: string;
}

export interface DashboardSummary {
  metrics: DashboardMetrics;
  aiSummary: string;
  trendingThemes: TrendingTheme[];
  alerts: DashboardAlert[];
  sourceBreakdown: SourceBreakdown[];
  volumeData: number[];
  volumeLabels: string[];
}

export type SupportCategory =
  | "Payments"
  | "Account"
  | "Technical"
  | "Billing"
  | "Shipping"
  | "Other";

export type SupportPriority = "Low" | "Medium" | "High" | "Critical";

export type SupportSentiment =
  | "Positive"
  | "Neutral"
  | "Frustrated"
  | "Angry";

export interface TriageRequest {
  message: string;
}

export interface TriageResult {
  category: SupportCategory;
  priority: SupportPriority;
  sentiment: SupportSentiment;
  summary: string;
  suggestedTeam: string;
}

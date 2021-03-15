export interface AgentGroup {
  name: string;
  description: string;
  id: string;
  isDeleted?: boolean;
  isEnabled: boolean;
  timestamp?: string;
  updatedBy?: string;
  updatedOn?: string;
  createdBy?: string;
  createdOn?: string;
  deleteOn?: unknown;
  deletedBy?: string;
}

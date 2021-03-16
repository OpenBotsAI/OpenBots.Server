export interface AgentGroup {
  // agentId and agentName replace by id and ItemName because of dropdown takes those properties
  agentId?: string; // these used for changing property name that's why its declared
  agentName?: string; //these used for changing property name that's why its declared
  itemName: string;
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

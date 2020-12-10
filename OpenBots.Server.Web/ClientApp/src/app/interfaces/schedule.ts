export interface Schedule {
  agentId: string;
  agentName: string;
  // processName: string;
  automationId: string;
  automationName: string;
  createdBy: string;
  createdOn: string;
  cronExpression: string;
  deleteOn?: any;
  deletedBy?: string;
  endJobAtOccurence?: any;
  endJobOn?: any;
  expiryDate?: any;
  id: string;
  isDeleted: boolean;
  isDisabled: boolean;
  jobRecurEveryUnit?: any;
  lastExecution?: any;
  name: string;
  nextExecution?: any;
  noJobEndDate?: any;
  projectId: string;
  recurrence: boolean;
  recurrenceUnit?: any;
  startDate?: any;
  startJobOn?: any;
  startingType: string;
  status: string;
  timestamp: string;
  triggerName: string;
  updatedBy?: string;
  updatedOn?: any;
  processId?: string;
}

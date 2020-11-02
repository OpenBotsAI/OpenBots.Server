export interface ProcessLogs {
  message: string;
  messageTemplate: string;
  level: string;
  processLogTimeStamp: Date;
  exception: string;
  properties: string;
  jobId: string;
  processId: string;
  agentId: string;
  machineName: string;
  processName: string;
  logger: string;
  id: string;
  isDeleted: boolean;
  createdBy: string;
  createdOn: Date;
  deletedBy: string;
  deleteOn: Date;
  timestamp: string;
  updatedOn: Date;
  updatedBy: string;
}

export interface Queues {
  createdBy: string;
  createdOn: string;
  deleteOn?: any;
  deletedBy: string;
  description: string;
  id: string;
  isDeleted: boolean;
  name: string;
  timestamp: string;
  updatedBy?: any;
  updatedOn?: any;
  maxRetryCount?: number;
}

export interface Credential {
  certificate: string;
  createdBy: string;
  createdOn: string;
  deleteOn?: any;
  deletedBy: string;
  domain: string;
  endDate: Date;
  id: string;
  isDeleted: boolean;
  name: string;
  passwordHash: string;
  passwordSecret: string;
  provider: string;
  startDate: Date;
  timestamp: string;
  updatedBy?: any;
  updatedOn?: any;
  userName: string;
}

export interface IPFencing {
  id: string;
  isDeleted: boolean;
  createdBy: string;
  createdOn: string;
  deletedBy: string;
  deleteOn: string;
  timestamp: string;
  updatedOn: string;
  updatedBy: string;
  usage: string;
  rule: string;
  ipAddress: string;
  ipRange: string;
  headerName: string;
  headerValue: string;
  organizationId: string;
}

export interface Usage {
  name: string;
  value: number;
}

export interface Rule {
  name: string;
  value: number;
}

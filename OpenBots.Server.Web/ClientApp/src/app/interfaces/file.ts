export interface BinaryFile {
  contentType: string;
  correlationEntity: string;
  correlationEntityId?: any;
  createdBy: string;
  createdOn: string;
  deleteOn?: any;
  deletedBy?: string;
  folder?: any;
  hashCode: string;
  id: string;
  isDeleted?: boolean;
  name: string;
  organizationId: string;
  sizeInBytes: number;
  storagePath: string;
  storageProvider: string;
  timestamp?: string;
  updatedBy?: string;
  updatedOn?: string;
}

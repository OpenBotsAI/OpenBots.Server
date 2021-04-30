export interface StorageDrive {
  id: string;
  isDeleted: false;
  createdBy?: string;
  createdOn?: string;
  deletedBy?: string;
  deleteOn?: string;
  timestamp?: string;
  updatedOn?: string;
  updatedBy?: string;
  name: string;
  fileStorageAdapterType: string;
  organizationId: string;
  storagePath: string;
  storageSizeInBytes: number;
  maxStorageAllowedInBytes: number;
  isDefault: boolean;
}

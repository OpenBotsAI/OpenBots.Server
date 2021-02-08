export interface FileManager {
  content: any;
  contentType: string;
  createdBy?: string;
  createdOn?: string;
  files?: any;
  fullStoragePath: string;
  hasChild: boolean;
  id: string;
  isFile: false;
  name: string;
  parentId: string;
  size: number;
  storageDriveId: string;
  storagePath: string;
  updatedOn?: any;
}

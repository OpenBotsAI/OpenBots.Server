import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'ngx-all-files',
  templateUrl: './all-files.component.html',
  styleUrls: ['./all-files.component.scss'],
})
export class AllFilesComponent implements OnInit {
  fileManger = [
    {
      id: 'ffb1ff0b-bf66-4580-94f2-6fd259edf2ee',

      name: 'Colorful Background Wallpaper.jpg',

      size: 240751,

      storagePath: 'Files\\Folder2',

      isChild: true,

      contentType: 'image/jpeg',

      createdBy: 'nicole.carrero@openbots.ai',

      createdOn: '2021-01-27T15:54:33.1132244',

      isFile: true,

      parentId: '4ad4e87a-61a5-49cc-ae84-421d05309f03',

      fullStoragePath: 'Files\\Folder2\\Colorful Background Wallpaper.jpg',

      content: null,

      file: null,
    },

    {
      id: 'f0e30d66-d2de-46ab-8ca7-83393fddffad',

      name: 'Process Execution Logs.postman_collection.json',

      size: 9815,

      storagePath: 'Files',

      isChild: true,

      contentType: 'application/json',

      createdBy: null,

      createdOn: '2021-01-26T23:39:24.4603478',

      isFile: true,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\Process Execution Logs.postman_collection.json',

      content: null,

      file: null,
    },

    {
      id: '8b788f35-261d-406a-8403-8ef7539e1eb6',

      name: 'HelloWorld2_1.0.0.nupkg',

      size: 6530,

      storagePath: 'Files',

      isChild: true,

      contentType: 'application/octet-stream',

      createdBy: null,

      createdOn: '2021-01-26T23:42:05.4929055',

      isFile: true,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\HelloWorld2_1.0.0.nupkg',

      content: null,

      file: null,
    },

    {
      id: '17956e0b-7e93-4a67-bc19-963fc834002e',

      name: 'fileManager details.docx',

      size: 12476,

      storagePath: 'Files',

      isChild: true,

      contentType:
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',

      createdBy: null,

      createdOn: '2021-01-26T23:35:38.913503',

      isFile: true,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\fileManager details.docx',

      content: null,

      file: null,
    },

    {
      id: '5b051f41-ecee-4148-8490-b40deb3ac68f',

      name: 'Sample Work Sheet.xlsx',

      size: 6172,

      storagePath: 'Files',

      isChild: true,

      contentType:
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',

      createdBy: 'nicole.carrero@openbots.ai',

      createdOn: '2021-01-27T15:50:47.8588225',

      isFile: true,

      parentId: '090f2d80-333d-434a-825a-127674899fc9',

      fullStoragePath: 'Files\\Folder2\\SubFolder2\\Sample Work Sheet.xlsx',

      content: null,

      file: null,
    },

    {
      id: '04dfac62-7fad-408f-b6c3-dc9040e97008',

      name: 'ProcessLogs.postman_collection.json',

      size: 4192,

      storagePath: 'Files',

      isChild: true,

      contentType: 'application/json',

      createdBy: null,

      createdOn: '2021-01-26T22:19:31.4327703',

      isFile: true,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\ProcessLogs.postman_collection.json',

      content: null,

      file: null,
    },

    {
      id: '81329e05-933f-4a9a-8413-087b0e56994c',

      name: 'Folder1',

      size: 0,

      storagePath: 'Files',

      isChild: true,

      contentType: 'Folder',

      createdBy: null,

      createdOn: '2021-01-27T00:00:38.0017883',

      isFile: false,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\Folder1',

      content: null,

      file: null,
    },

    {
      id: '4ad4e87a-61a5-49cc-ae84-421d05309f03',

      name: 'Folder2',

      size: 0,

      storagePath: 'Files',

      isChild: true,

      contentType: 'Folder',

      createdBy: null,

      createdOn: '2021-01-27T00:14:16.6897604',

      isFile: false,

      parentId: '37a01356-7514-47a2-96ce-986faadd628e',

      fullStoragePath: 'Files\\Folder2',

      content: null,

      file: null,
    },

    {
      id: '3ce7a77d-8ea7-44fc-9b19-6884a9fdf5bc',

      name: 'SubFolder2',

      size: 0,

      storagePath: 'Files\\Folder2',

      isChild: true,

      contentType: 'Folder',

      createdBy: 'nicole.carrero@openbots.ai',

      createdOn: '2021-01-27T15:49:47.736139',

      isFile: false,

      parentId: '4ad4e87a-61a5-49cc-ae84-421d05309f03',

      fullStoragePath: 'Files\\Folder2\\SubFolder2',

      content: null,

      file: null,
    },
  ];
  name: any = [];
  size: any = [];
  contentType: any = [];
  createdOn: any = [];
  fullStoragePath: any = [];
  constructor() {}

  ngOnInit(): void {
    this.gotodetail(
      this.fileManger[0].name,
      this.fileManger[0].size,
      this.fileManger[0].contentType,
      this.fileManger[0].createdOn,
      this.fileManger[0].fullStoragePath
    );
  }

  gotodetail(name, size, contentType, createdOn, fullStoragePath) {
    this.name = name;
    this.size = size;
    this.contentType = contentType;
    this.createdOn = createdOn;
    this.fullStoragePath = fullStoragePath;
  }
}

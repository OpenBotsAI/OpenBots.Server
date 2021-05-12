import { Component, EventEmitter, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { FileSaverService } from 'ngx-filesaver';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { Page } from '../../../interfaces/paginateInstance';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import {
  UploaderOptions,
  UploadFile,
  UploadInput,
  UploadOutput,
} from 'ngx-uploader';
import { HttpService } from '../../../@core/services/http.service';
import { FileManager } from '../../../interfaces/fileManager';
import { FileManagerApiUrl } from '../../../webApiUrls';
@Component({
  selector: 'ngx-all-files',
  templateUrl: './all-files.component.html',
  styleUrls: ['./all-files.component.scss'],
})
export class AllFilesComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  fileSize = false;
  ///// end declartion////
  filesFormgroup: FormGroup;
  filesCreateFolderFromgroup: FormGroup;
  fileManger: FileManager[] = [];
  fileID: string;
  fileName: string;
  size: number;
  contentType: string;
  createdOn: string;
  fullStoragePath: string;
  floderName: string;
  bread: FileManager[] = [];
  fileType: string;
  page: Page = {};
  itemsPerPage: ItemsPerPage[] = [];
  showDownloadbtn = false;
  Downloadbtn = false;
  //// select row ///
  HighlightRow: number;
  ClickedRow: any;
  filterOrderBy: string;
  driveName: string;
  driveId: string;
  currentParentId: string;
  uploadedFilesArr: any[] = [];
  isHidden = true;
  renameId: string;
  storageDriveArr: FileManager[] = [];
  storageDriveForm: FormGroup;
  retrictedFilesarr = ['.bat', '.exe', '.com', '.vbs'];
  isDownloadButton = false;
  pageNumberRecord: number[] = [];
  isFileUploaded = false;
  isFolderCreated = false;
  isDeleted = false;
  constructor(
    private fileSaverService: FileSaverService,
    private formBuilder: FormBuilder,
    private dialogService: DialogService,
    private helperService: HelperService,
    private httpService: HttpService
  ) {
    this.ClickedRow = function (index) {
      this.HighlightRow = index;
    };
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.itemsPerPage = this.helperService.getItemsPerPage();
    this.getAllStorageDrives();
    this.storageDriveForm = this.initializeForm();
  }
  initializeForm() {
    return this.formBuilder.group({
      id: [''],
    });
  }
  getAllStorageDrives(): void {
    this.httpService
      .get(`${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}`)
      .subscribe((response) => {
        if (response && response.items && response.items.length) {
          this.storageDriveArr = [...response.items];
          this.driveId = this.storageDriveArr[0].id;
          this.driveName = this.storageDriveArr[0].name;
          this.storageDriveForm.patchValue(this.storageDriveArr[0]);
        }
        this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
      });
  }

  getStorageDriveByDriveId(
    pageNumber: number,
    pageSize: number,
    orderBy?: string
  ): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    let url: string;
    if (this.currentParentId) {
      if (orderBy)
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.currentParentId}'`;
      else
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.currentParentId}'`;
    } else {
      if (orderBy)
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.driveId}'`;
      else
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.driveId}'`;
    }

    this.httpService.get(url).subscribe((response) => {
      this.page.totalCount = response.totalCount;
      if (response && response.items && response.items.length) {
        this.fileManger = [];
        this.fileManger = [...response.items];
      } else this.fileManger = [];
    });
  }

  onStorageDriveChange(event) {
    if (event) {
      this.driveId = event.id;
      this.driveName = event.name;
      this.currentParentId = null;
      this.bread = [];
      this.pageNumberRecord = [];
      this.page.pageNumber = 1;
      this.page.pageSize = 5;
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    }
  }

  refreshFileManager() {
    this.currentParentId = null;
    this.bread = [];
    this.pageNumberRecord = [];
    this.page.pageNumber = 1;
    this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
  }

  driveDocumentDetail(file): void {
    if (file && file.isFile) {
      this.isHidden = false;
    } else {
      this.isHidden = true;
    }
    if (file) {
      this.showDownloadbtn = true;
      if (file.isFile == true) {
        this.isDownloadButton = true;
      } else if (file.isFile == false) {
        this.isDownloadButton = false;
      }
      this.fileID = file.id;
      this.fileName = file.name;
      this.size = file.size;
      this.contentType = file.contentType;
      this.createdOn = file.createdOn;
      this.fullStoragePath = file.fullStoragePath;
    }
  }

  deleteFiles(ref: TemplateRef<any>): void {
    this.dialogService.openDialog(ref);
  }
  openRenameDialog(ref: TemplateRef<any>): void {
    this.filesFormgroup = this.formBuilder.group({
      name: [''],
    });
    this.fileName = this.fileName.split('.')[0];
    this.filesFormgroup.patchValue({ name: this.fileName });
    this.dialogService.openDialog(ref);
  }

  openCreateFolderDialog(ref: TemplateRef<any>): void {
    this.filesCreateFolderFromgroup = this.formBuilder.group({
      name: [''],
      StoragePath: [''],
      isFile: [false],
    });
    this.dialogService.openDialog(ref);
  }
  createFolder(ref) {
    this.isFolderCreated = true;
    let storagePath = '';
    this.bread.forEach((item) => (storagePath += '/' + item.name));
    let formData = new FormData();
    formData.append('Name', this.filesCreateFolderFromgroup.value.name);
    formData.append('StoragePath', `${this.driveName}` + storagePath);
    formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    this.httpService
      .post(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.folders}`,
        formData,
        {
          observe: 'response',
        }
      )
      .subscribe(
        (data) => {
          if (data && data.status === 200) {
            this.isFolderCreated = false;
            this.page.pageNumber = 1;
            this.getStorageDriveByDriveId(
              this.page.pageNumber,
              this.page.pageSize
            );
            this.httpService.success('New folder created successfully');
            ref.close();
          }
        },
        () => (this.isFolderCreated = false)
      );
  }

  get fc() {
    return this.filesCreateFolderFromgroup.controls;
  }
  get f() {
    return this.filesFormgroup.controls;
  }

  openUploadFileDialog(ref: TemplateRef<any>): void {
    this.dialogService.openDialog(ref);
  }

  onUploadOutput(output: UploadOutput): void {
    let uplaodFIle;
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) {
            this.fileSize = true;
          } else {
            this.fileSize = false;
          }
          uplaodFIle = output.file.name;
          for (let extension of this.retrictedFilesarr) {
            if (output.file.name.includes(extension)) {
              this.httpService.error('This File format is not allowed');
              return;
            }
          }
          this.uploadedFilesArr.push(output.file);
        }
        break;
    }
  }
  UploadFile(ref): void {
    this.isFileUploaded = true;
    let storagePath = this.driveName;
    if (this.bread && this.bread.length)
      this.bread.forEach((item) => (storagePath += `/${item.name}`));
    let formData = new FormData();
    formData.append('StoragePath', storagePath);
    for (let data of this.uploadedFilesArr) {
      formData.append('Files', data.nativeFile, data.nativeFile.name);
    }
    this.httpService
      .post(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.files}`,
        formData,
        {
          observe: 'response',
        }
      )
      .subscribe(
        (data: any) => {
          if (data && data.status === 200) {
            this.isFileUploaded = false;
            this.uploadedFilesArr = [];
            this.getStorageDriveByDriveId(
              this.page.pageNumber,
              this.page.pageSize
            );
          }
          ref.close();
        },
        () => (this.isFileUploaded = false)
      );
  }

  cancelUpload(id: string): void {
    this.uploadInput.emit({ type: 'cancel', id: id });
  }

  removeFile(id: string): void {
    this.uploadInput.emit({ type: 'remove', id: id });
  }

  removeAllFiles(): void {
    this.uploadInput.emit({ type: 'removeAll' });
  }
  renameFileName(ref) {
    this.httpService
      .put(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.folders}/${this.fileID}/${FileManagerApiUrl.rename}`,
        this.filesFormgroup.value,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status === 200) {
          this.getStorageDriveByDriveId(
            this.page.pageNumber,
            this.page.pageSize
          );
          this.httpService.success('Renamed successfully');
          ref.close();
        }
      });
  }

  onDown() {
    this.httpService
      .get(
        `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.download}`,
        {
          responseType: 'blob',
          observe: 'response',
        }
      )
      .subscribe((response) => {
        this.fileSaverService.save(
          response.body,
          response.headers
            .get('content-disposition')
            .split(';')[1]
            .split('=')[1]
            .replace(/\"/g, '')
        );
      });
  }

  onBreadCrumb(i: number): void {
    for (let index in this.bread) {
      if (+index > i) {
        this.bread.splice(+index, this.bread.length - i);
        this.currentParentId = this.bread[i].id;
        this.page.pageNumber =
          this.pageNumberRecord[this.pageNumberRecord.length - 1];
        this.pageNumberRecord.splice(+index, this.pageNumberRecord.length - 1);
        this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
        this.floderName = this.bread[i].name;
      }
    }
  }

  onClickUp() {
    if (this.bread && this.bread.length > 1) {
      this.bread.splice(this.bread.length - 1, 1);
      this.floderName = this.bread[this.bread.length - 1].name;
      this.currentParentId = this.bread[this.bread.length - 1].id;
      this.page.pageNumber =
        this.pageNumberRecord[this.pageNumberRecord.length - 1];
      this.pageNumberRecord.splice(this.pageNumberRecord.length - 1, 1);
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    } else {
      this.currentParentId = null;
      this.page.pageNumber = this.pageNumberRecord[0];
      this.floderName = null;
      this.bread = [];
      this.pageNumberRecord = [];
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    }
  }

  getByIdFile(id: string): void {
    this.httpService
      .get(`${FileManagerApiUrl.files}?&$filter=ParentId+eq+guid'${id}'`)
      .subscribe((response) => {
        if (response && response.items) {
          this.fileManger = [];
          this.fileManger = [...response.items];
        }
        this.page.totalCount = response.totalCount;
      });
  }

  onDoubleClick(files) {
    this.HighlightRow = null;
    this.showDownloadbtn = false;
    if (files && files.isFile == false) {
      if (this.floderName != files.name) {
        this.floderName = files.name;
        this.bread.push(files);
        this.pageNumberRecord.push(this.page.pageNumber);
      }
      this.page.pageNumber = 1;
      this.currentParentId = files.id;
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    }
  }

  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.getStorageDriveByDriveId(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+asc`
      );
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.getStorageDriveByDriveId(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+desc`
      );
    }
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.bread && !this.bread.length) {
      this.pageNumberRecord.push(event);
    }
    if (this.filterOrderBy) {
      this.getStorageDriveByDriveId(
        event,
        this.page.pageSize,
        this.filterOrderBy
      );
    } else {
      this.getStorageDriveByDriveId(event, this.page.pageSize);
    }
  }

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      this.getStorageDriveByDriveId(
        this.page.pageNumber,
        this.page.pageSize,
        this.filterOrderBy
      );
    } else
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllFilesAndFdlders(top, skip, orderBy);
  }

  getAllFilesAndFdlders(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `${FileManagerApiUrl.files}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${FileManagerApiUrl.files}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items) {
        this.fileManger = [...response.items];
        this.page.totalCount = response.totalCount;
      } else this.fileManger = [];
    });
  }
  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }

  closeFileUploadPopup(ref) {
    ref.close();
    this.uploadedFilesArr = [];
  }

  onDelete(ref): void {
    this.isDeleted = true;
    let contentType;
    if (this.isHidden) contentType = FileManagerApiUrl.folders;
    else contentType = FileManagerApiUrl.files;
    this.httpService
      .delete(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${contentType}/${this.fileID}`,
        {
          observe: 'response',
        }
      )
      .subscribe(
        (response) => {
          if (response && response.status === 200) {
            this.HighlightRow = null;
            this.showDownloadbtn = false;
            this.isDeleted = false;
            this.isDownloadButton = false;
            this.getStorageDriveByDriveId(
              this.page.pageNumber,
              this.page.pageSize
            );
          }
          this.httpService.success('Deleted successfully');
          ref.close();
        },
        () => (this.isDeleted = false)
      );
  }
}

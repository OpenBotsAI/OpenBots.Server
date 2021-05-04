import { HttpResponse } from '@angular/common/http';
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
  FolderIDs: any = [];
  ChildFolderFlag: boolean;
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any;
  native_file_name: any;
  fileSize = false;
  ///// end declartion////
  filesFormgroup: FormGroup;
  filesCreateFolderFromgroup: FormGroup;
  fileManger: FileManager[] = [];
  fileID: any = [];
  fileName: string;
  size: any = [];
  contentType: any = [];
  createdOn: any = [];
  fullStoragePath: any = [];
  floderName: any;
  bread: FileManager[] = [];
  isSubmitted = false;
  fileType: string;
  showpage: any = [];
  sortDir = 1;
  showData;
  feild_name: any = [];
  page: Page = {};
  get_perPage: boolean = false;
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  itemsPerPage: ItemsPerPage[] = [];
  showDownloadbtn: boolean = false;
  Downloadbtn: boolean = false;
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
  savePageNumber = 1;
  retrictedFilesarr = ['.bat', '.exe', '.com', '.vbs'];
  isDownloadButton = false;
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
    // this.getdriveName();
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
    }
    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    else {
      if (orderBy)
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.driveId}'`;
      else
        url = `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${this.driveId}'`;
    }

    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    this.httpService.get(url).subscribe((response) => {
      this.page.totalCount = response.totalCount;
      if (response && response.items && response.items.length) {
        // this.bread = [];
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
      this.page.pageNumber = 1;
      this.page.pageSize = 5;
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    }
  }

  getdriveName(): void {
    this.httpService
      .get(`${FileManagerApiUrl.files}/${FileManagerApiUrl.drive}`, {
        observe: 'response',
      })
      .subscribe((response) => {
        if (response && response.status === 200) {
          this.driveName = response.body.name;
          this.driveId = response.body.id;
          this.currentParentId = response.body.id;
          this.getFilterPagination(
            this.page.pageNumber,
            this.page.pageSize,
            this.driveId
          );
        }
      });
  }
  // old function used for old user manager
  // this function get pagination using parent-id
  getFilterPagination(
    pageNumber: number,
    pageSize: number,
    id: string,
    orderBy?: string
  ): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    let url: string;
    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    if (orderBy)
      url = `${FileManagerApiUrl.files}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    else
      url = `${FileManagerApiUrl.files}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    this.httpService.get(url).subscribe((response) => {
      this.page.totalCount = response.totalCount;
      if (response && response.items && response.items.length) {
        // this.bread = [];
        this.fileManger = [];
        this.fileManger = [...response.items];
      } else this.fileManger = [];
    });
  }

  driveDocumentDetail(file): void {
    console.log('file', file);
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

  onClickFiles(): void {
    this.bread = [];
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    // this.getdriveName();
  }

  deleteFiles(ref: TemplateRef<any>): void {
    this.dialogService.openDialog(ref);
  }
  openRenameDialog(ref: TemplateRef<any>): void {
    this.filesFormgroup = this.formBuilder.group({
      name: [''],
    });
    // this.fileName = this.fileName.split('.')[0];
    // console.log('xtension', this.fileName);
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
    let storagePath = '';
    this.bread.forEach((item) => (storagePath += '/' + item.name));
    let formData = new FormData();
    formData.append('Name', this.filesCreateFolderFromgroup.value.name);
    formData.append('StoragePath', `${this.driveName}` + storagePath);
    formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    this.httpService
      .post(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.folders}`,
        // `${FileManagerApiUrl.files}`,
        formData,
        {
          observe: 'response',
        }
      )
      .subscribe((data) => {
        if (data && data.status === 200) {
          if (this.bread && !this.bread.length)
            this.getStorageDriveByDriveId(1, this.page.pageSize);
          else {
            this.getByIdFile(this.bread[this.bread.length - 1].id);
          }
          this.httpService.success('New folder created successfully');
          ref.close();
        }
      });
  }

  // createFolder(ref) {
  //   let storagePath = '';
  //   this.bread.forEach((item) => (storagePath += '/' + item.name));
  //   let formData = new FormData();
  //   formData.append('Name', this.filesCreateFolderFromgroup.value.name);
  //   formData.append('StoragePath', `${this.driveName}` + storagePath);
  //   formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
  //   this.httpService
  //     .post(
  //       ` ${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.folders}`,
  //       // `${FileManagerApiUrl.files}`,
  //       formData,
  //       {
  //         observe: 'response',
  //       }
  //     )
  //     .subscribe((data) => {
  //       if (data && data.status === 200) {
  //         if (this.bread && !this.bread.length)
  //           this.getFilterPagination(
  //             1,
  //             this.page.pageSize,
  //             this.currentParentId
  //           );
  //         else {
  //           this.getByIdFile(this.bread[this.bread.length - 1].id);
  //         }
  //         this.httpService.success('New folder created successfully');
  //         ref.close();
  //       }
  //     });
  // }

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
          console.log('file', output);
          uplaodFIle = output.file.name;
          // for (let extension of this.retrictedFilesarr) {
          //   if (extension == output.file.name.includes()) {
          //     this.httpService.error('This File format is not allowed');
          //   }
          // }

          if (
            uplaodFIle.includes('.BAT') ||
            uplaodFIle.includes('.bat') ||
            uplaodFIle.includes('.exe') ||
            uplaodFIle.includes('.com') ||
            uplaodFIle.includes('.VBS') ||
            uplaodFIle.includes('.vbs') ||
            uplaodFIle.includes('.COM')
          ) {
            // this.submitted = false
            this.httpService.error('This File format is not allowed ');
          } else {
            this.uploadedFilesArr.push(output.file);
          }
        }
        break;
    }
  }
  UploadFile(ref): void {
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
        // `${FileManagerApiUrl.files}?driveName=${this.driveName}`,
        // `${FileManagerApiUrl.files}`,
        formData,
        {
          observe: 'response',
        }
      )
      .subscribe((data: any) => {
        if (data && data.status === 200) {
          this.uploadedFilesArr = [];
          // if (this.bread.length) {
          this.getStorageDriveByDriveId(
            this.page.pageNumber,
            this.page.pageSize
          );
          // } else {
          // this.getFilterPagination(
          //   this.page.pageNumber,
          //   this.page.pageSize,
          //   this.driveId
          // );
          // }
        }
        ref.close();
      });
  }
  // UploadFile(ref): void {
  //   let storagePath = this.driveName;
  //   if (this.bread && this.bread.length)
  //     this.bread.forEach((item) => (storagePath += `/${item.name}`));
  //   let formData = new FormData();
  //   for (let data of this.uploadedFilesArr) {
  //     formData.append('Files', data.nativeFile, data.nativeFile.name);
  //     formData.append('StoragePath', storagePath);
  //   }
  //   this.httpService
  //     .post(
  //       `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${FileManagerApiUrl.files}`,
  //       // `${FileManagerApiUrl.files}?driveName=${this.driveName}`,
  //       // `${FileManagerApiUrl.files}`,
  //       formData,
  //       {
  //         observe: 'response',
  //       }
  //     )
  //     .subscribe((data: any) => {
  //       if (data && data.status === 200) {
  //         this.uploadedFilesArr = [];
  //         if (this.bread.length) {
  //           this.getFilterPagination(
  //             this.page.pageNumber,
  //             this.page.pageSize,
  //             this.currentParentId
  //           );
  //         } else {
  //           this.getFilterPagination(
  //             this.page.pageNumber,
  //             this.page.pageSize,
  //             this.driveId
  //           );
  //         }
  //       }
  //       ref.close();
  //     });
  // }
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
        // `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.rename}?driveName=${this.driveName}`,
        // `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.rename}`,
        this.filesFormgroup.value,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status === 200) {
          // if (this.bread.length) {
          this.getStorageDriveByDriveId(
            this.page.pageNumber,
            this.page.pageSize
          );
          // } else {
          // this.getFilterPagination(
          //   this.page.pageNumber,
          //   this.page.pageSize,
          //   this.currentParentId
          // );

          // }

          this.httpService.success('Renamed successfully');
          ref.close();
        }
      });
  }

  // renameFileName(ref) {
  //   this.httpService
  //     .put(
  //       // `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.rename}?driveName=${this.driveName}`,
  //       `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.rename}`,
  //       this.filesFormgroup.value,
  //       { observe: 'response' }
  //     )
  //     .subscribe((response) => {
  //       if (response && response.status === 200) {
  //         if (this.bread.length) {
  //           this.getFilterPagination(
  //             this.page.pageNumber,
  //             this.page.pageSize,
  //             this.bread[this.bread.length - 1].id
  //           );
  //         } else {
  //           this.getFilterPagination(
  //             this.page.pageNumber,
  //             this.page.pageSize,
  //             this.currentParentId
  //           );
  //         }

  //         this.httpService.success('Renamed successfully');
  //         ref.close();
  //       }
  //     });
  // }

  onDown() {
    let fileName: string;
    this.httpService
      .get(
        // `${FileManagerApiUrl.files}/${this.fileID}/${FileManagerApiUrl.download}?driveName=${this.driveName}`
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

  getFileId(i: number): void {
    for (let index in this.bread) {
      if (+index > i) {
        this.bread.splice(+index, this.bread.length - i);
        this.currentParentId = this.bread[i].id;
        this.getStorageDriveByDriveId(1, this.page.pageSize);
        // this.getFilterPagination(1, this.page.pageSize, this.bread[i].id);
        this.floderName = this.bread[i].name;
      }
    }
  }

  onClickUp() {
    if (this.bread && this.bread.length > 1) {
      this.bread.splice(this.bread.length - 1, 1);
      this.floderName = this.bread[this.bread.length - 1].name;
      this.currentParentId = this.bread[this.bread.length - 1].id;
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    } else {
      this.currentParentId = null;
      this.page.pageNumber = this.savePageNumber;
      this.floderName = null;
      this.bread = [];
      // this.bread.splice(this.bread.length - 1, 1);
      this.getStorageDriveByDriveId(this.page.pageNumber, this.page.pageSize);
    }
  }

  getByIdFile(id: string): void {
    this.httpService
      .get(
        // `${FileManagerApiUrl.files}?driveName=${this.driveName}&$filter=ParentId+eq+guid'${id}'`
        `${FileManagerApiUrl.files}?&$filter=ParentId+eq+guid'${id}'`
      )
      .subscribe((response) => {
        if (response && response.items) {
          this.fileManger = [];
          this.fileManger = [...response.items];
        }
        this.page.totalCount = response.totalCount;
      });
  }

  fileFolder(files) {
    this.HighlightRow = null;
    this.showDownloadbtn = false;
    debugger;
    if (files && files.isFile == false) {
      if (this.floderName != files.name) {
        this.floderName = files.name;
        this.bread.push(files);
      }
      this.FolderIDs = files.id;
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
    if (this.bread && !this.bread.length) {
      this.savePageNumber = event;
    }
    this.page.pageNumber = event;
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

  // onClickUp() {
  //   if (this.bread && this.bread.length > 1) {
  //     this.bread.splice(this.bread.length - 1, 1);
  //     this.floderName = this.bread[this.bread.length - 1].name;
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.bread[this.bread.length - 1].id
  //     );
  //   } else {
  //     this.bread.splice(this.bread.length - 1, 1);
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.driveId
  //     );
  //   }
  // }

  // fileFolder(files) {
  //   this.HighlightRow = null;
  //   this.showDownloadbtn = false;
  //   if (files && files.isFile == false) {
  //     if (this.floderName != files.name) {
  //       this.floderName = files.name;
  //       this.bread.push(files);
  //     }
  //     this.FolderIDs = files.id;
  //     this.page.pageNumber = 1;
  //     this.currentParentId = files.id;
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       files.id
  //     );
  //   }
  // }

  // onSortClick(event, param: string): void {
  //   let target = event.currentTarget,
  //     classList = target.classList;
  //   if (classList.contains('fa-chevron-up')) {
  //     classList.remove('fa-chevron-up');
  //     classList.add('fa-chevron-down');
  //     this.filterOrderBy = `${param}+asc`;
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.currentParentId,
  //       `${param}+asc`
  //     );
  //   } else {
  //     classList.remove('fa-chevron-down');
  //     classList.add('fa-chevron-up');
  //     this.filterOrderBy = `${param}+desc`;
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.currentParentId,
  //       `${param}+desc`
  //     );
  //   }
  // }

  // pageChanged(event): void {
  //   this.page.pageNumber = event;
  //   if (this.filterOrderBy) {
  //     this.getFilterPagination(
  //       event,
  //       this.page.pageSize,
  //       this.currentParentId,
  //       `${this.filterOrderBy}`
  //     );
  //   } else {
  //     this.getFilterPagination(event, this.page.pageSize, this.currentParentId);
  //   }
  // }

  // selectChange(event): void {
  //   this.page.pageSize = +event.target.value;
  //   this.page.pageNumber = 1;
  //   if (event.target.value && this.filterOrderBy) {
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.currentParentId,
  //       this.filterOrderBy
  //     );
  //   } else
  //     this.getFilterPagination(
  //       this.page.pageNumber,
  //       this.page.pageSize,
  //       this.currentParentId
  //     );
  // }

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
    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    if (orderBy)
      url = `${FileManagerApiUrl.files}?&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${FileManagerApiUrl.files}?&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    // url = `${FileManagerApiUrl.files}?driveName=${this.driveName}&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
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

  // onDelete(ref) {
  //   this.httpService
  //     .delete(
  //       // `${FileManagerApiUrl.files}/${this.fileID}?driveName=${this.driveName}`,
  //       `${FileManagerApiUrl.files}/${this.fileID}?`,
  //       {
  //         observe: 'response',
  //       }
  //     )
  //     .subscribe((response) => {
  //       if (response && response.status === 200) {
  //         if (!this.bread.length)
  //           this.getFilterPagination(1, this.page.pageSize, this.driveId);
  //         else {
  //           this.getFilterPagination(
  //             1,
  //             this.page.pageSize,
  //             this.currentParentId
  //           );
  //         }
  //         this.httpService.success('Deleted successfully');
  //         ref.close();
  //       }
  //     });
  // }

  onDelete(ref) {
    let contentType;
    if (this.isHidden) contentType = FileManagerApiUrl.folders;
    else contentType = FileManagerApiUrl.files;
    this.httpService
      .delete(
        `${FileManagerApiUrl.storage}/${FileManagerApiUrl.drives}/${this.driveId}/${contentType}/${this.fileID}`,
        // `${FileManagerApiUrl.files}/${this.fileID}?driveName=${this.driveName}`,
        // `${FileManagerApiUrl.files}/${this.fileID}?`,
        {
          observe: 'response',
        }
      )
      .subscribe((response) => {
        if (response && response.status === 200) {
          this.HighlightRow = null;
          this.getStorageDriveByDriveId(
            this.page.pageNumber,
            this.page.pageSize
          );
          // if (!this.bread.length)
          //   this.getFilterPagination(1, this.page.pageSize, this.driveId);
          // else {
          //   this.getFilterPagination(
          //     1,
          //     this.page.pageSize,
          //     this.currentParentId
          //   );
        }
        this.httpService.success('Deleted successfully');
        ref.close();
        // }
      });
  }
}

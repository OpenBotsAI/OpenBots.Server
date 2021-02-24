import { HttpResponse } from '@angular/common/http';
import { Component, EventEmitter, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { FileSaverService } from 'ngx-filesaver';
import { DialogService } from '../../../@core/dialogservices';
import { FileManagerService } from '../fileManager.service';
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
  name: any = [];
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

  constructor(
    protected fileManagerService: FileManagerService,
    private FileSaverService: FileSaverService,
    private toastrService: NbToastrService,
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
    // this.pagination(this.page.pageNumber, this.page.pageSize);
    this.getdriveName();
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  getdriveName(): void {
    this.httpService
      .get(`files/drive?driveName=Files`, { observe: 'response' })
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
    if (orderBy)
      url = `files?driveName=Files&$orderby=${orderBy}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    else
      url = `files?driveName=Files&$orderby=createdOn+desc&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;

    this.httpService.get(url).subscribe((response) => {
      this.page.totalCount = response.totalCount;
      if (response && response.items && response.items.length) {
        // this.bread = [];
        this.fileManger = [];
        this.fileManger = [...response.items];
      } else this.fileManger = [];
    });
  }

  gotodetail(file): void {
    if (file && file.isFile) {
      this.isHidden = false;
    } else {
      this.isHidden = true;
    }
    if (file) {
      this.showDownloadbtn = true;
      if (file.isFile == true) {
        this.Downloadbtn = true;
      } else if (file.isFile == false) {
        this.Downloadbtn = false;
      }
      this.fileID = file.id;
      this.name = file.name;
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
    this.getdriveName();
    // this.getFilterPagination(1, this.page.pageSize, this.driveId);
  }

  deleteFiles(): void {
    // let filesurl = `/files/${id}?driveName=Files`;
    // this.fileManagerService.DeleteFileFloder(this.fileID)
    this.httpService
      .delete(`files/${this.fileID}?driveName=Files`)
      .subscribe(() => {
        // this.allFiles(5, 0);
        if (!this.bread.length)
          // this.pagination(1, this.page.pageSize);
          this.getFilterPagination(1, this.page.pageSize, this.driveId);
        else {
          // this.getByIdFile(this.bread[this.bread.length - 1].id);
          this.getFilterPagination(1, this.page.pageSize, this.currentParentId);
        }
        // this.getByIdFile(this.bread[this.bread.length - 1].id);
      });
  }
  openRenameDialog(ref: TemplateRef<any>): void {
    this.filesFormgroup = this.formBuilder.group({
      name: [''],
    });
    this.filesFormgroup.patchValue({ name: this.name });
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
      .post(`files?driveName=Files`, formData, { observe: 'response' })
      .subscribe((data) => {
        if (data && data.status === 200) {
          // if (this.filterOrderBy)
          //   this.pagination(
          //     this.page.pageNumber,
          //     this.page.pageSize,
          //     this.filterOrderBy
          //   );
          // else
          // this.pagination(1, this.page.pageSize);
          if (this.bread && !this.bread.length)
            this.getFilterPagination(
              1,
              this.page.pageSize,
              this.currentParentId
            );
          else {
            this.getByIdFile(this.bread[this.bread.length - 1].id);
          }
          ref.close();
        }
      });
    // this.fileManagerService.Createfolder(formData).subscribe((data: any) => {
    //   if (this.ChildFolderFlag == true) {
    //     this.getByIdFile(this.FolderIDs);
    //   } else if (this.ChildFolderFlag == false) {
    //     this.allFiles(5, 0);
    //   }
    //   // var n = email.match("/shareprocessemail");
    //   // this.allFiles(5, 0);
    //   //  this.getByIdFile(this.FolderIDs);
    //   ref.close();
    // });
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
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) {
            this.fileSize = true;
            // this.submitted = true;
          } else {
            this.fileSize = false;
            // this.submitted = false;
          }
          this.uploadedFilesArr.push(output.file);
          // this.native_file = output.file.nativeFile;
          // this.native_file_name = output.file.nativeFile.name;
          // this.show_upload = false;
        }
        break;
    }
  }
  UploadFile(ref): void {
    let storagePath = this.driveName;
    if (this.bread && this.bread.length)
      this.bread.forEach((item) => (storagePath += `/${item.name}`));
    let formData = new FormData();
    for (let data of this.uploadedFilesArr) {
      formData.append('Files', data.nativeFile, data.nativeFile.name);
      formData.append('StoragePath', storagePath);
    }
    // formData.append('Files', this.native_file, this.native_file_name);
    // formData.append('StoragePath', 'Files' + storagePath);
    // formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    // this.fileManagerService.Createfolder(formData)
    // let createfile = `/files?driveName=Files`;
    this.httpService
      .post(`files?driveName=Files`, formData, { observe: 'response' })
      .subscribe((data: any) => {
        if (data && data.status === 200) {
          this.uploadedFilesArr = [];
          if (this.bread.length) {
            this.getFilterPagination(
              this.page.pageNumber,
              this.page.pageSize,
              this.currentParentId
            );
          } else {
            // this.getdriveName();
            this.getFilterPagination(
              this.page.pageNumber,
              this.page.pageSize,
              this.driveId
            );
          }
        }

        // if (this.ChildFolderFlag == true) {
        //   // this.getByIdFile(this.FolderIDs);
        // } else if (this.ChildFolderFlag == false) {
        //   // this.allFiles(5, 0);
        //   this.pagination(this.page.pageNumber, this.page.pageSize);
        // }
        ref.close();
      });
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
        `files/${this.fileID}/rename?driveName=${this.driveName}`,
        this.filesFormgroup.value,
        { observe: 'response' }
      )
      .subscribe((response) => {
        // this.fileID
        if (response && response.status === 200) {
          ref.close();
          // if (this.bread.length) {
          //   this.getFilterPagination(
          //     this.page.pageNumber,
          //     this.page.pageSize,
          //     // this.currentParentId
          //     this.fileID
          //   );
          // } else {
          this.getFilterPagination(
            this.page.pageNumber,
            this.page.pageSize,
            this.currentParentId
          );
          // }
        }
      });
  }

  onDown() {
    let fileName: string;
    // this.fileManagerService
    //   .getFiledownload(this.fileID)
    // let downloadurl = `/files/${this.fileID}/download?driveName=Files`;
    this.httpService
      .get(`files/${this.fileID}/download?driveName=Files`)
      .subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this.FileSaverService.save(data.body, fileName);
      });
  }

  getFileId(i: number) {
    for (let abc in this.bread) {
      if (+abc > i) {
        this.bread.splice(+abc, this.bread.length - i);
        this.getFilterPagination(1, this.page.pageSize, this.bread[i].id);
        this.floderName = this.bread[i].name;
      }
    }
  }

  onClickUp() {
    if (this.bread && this.bread.length > 1) {
      this.bread.splice(this.bread.length - 1, 1);
      this.floderName = this.bread[this.bread.length - 1].name;
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.bread[this.bread.length - 1].id
      );
    } else {
      this.bread.splice(this.bread.length - 1, 1);
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.driveId
      );
    }
  }

  getByIdFile(id: string): void {
    // let filesurl = `/files?driveName=Files&$filter=${parentId}`;
    // this.fileManagerService
    //   .getFileFloder(`ParentId+eq+guid'${id}'`)
    // `files?driveName=Files&$orderby=${orderBy}}&$top=${top}&$skip=${skip}&$filter=ParentId+eq+guid'${id}'`;
    this.httpService
      .get(`files?driveName=Files&$filter=ParentId+eq+guid'${id}'`)
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
    if (files && files.isFile == false) {
      if (this.floderName != files.name) {
        this.floderName = files.name;
        this.bread.push(files);
      }
      this.FolderIDs = files.id;
      this.page.pageNumber = 1;
      this.currentParentId = files.id;
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        files.id
      );
    }
  }
  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.currentParentId,
        `${param}+asc`
      );
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.currentParentId,
        `${param}+desc`
      );
    }
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy) {
      this.getFilterPagination(
        event,
        this.page.pageSize,
        this.currentParentId,
        `${this.filterOrderBy}`
      );
    } else {
      this.getFilterPagination(event, this.page.pageSize, this.currentParentId);
    }

    // if (this.filterOrderBy)
    //   this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    // else this.pagination(event, this.page.pageSize);
  }

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      // this.pagination(
      //   this.page.pageNumber,
      //   this.page.pageSize,
      //   `${this.filterOrderBy}`
      // );
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.currentParentId,
        this.filterOrderBy
      );
    } else
      this.getFilterPagination(
        this.page.pageNumber,
        this.page.pageSize,
        this.currentParentId
      );
    // this.pagination(this.page.pageNumber, this.page.pageSize);
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllFilesAndFdlders(top, skip, orderBy);
  }

  getAllFilesAndFdlders(top: number, skip: number, orderBy?: string): void {
    // let filesurl = `/files?driveName=Files&$orderby=${name}&$top=${tpage}&$skip=${spage}`;
    let url: string;
    if (orderBy)
      url = `files?driveName=Files&$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `files?driveName=Files&$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items) {
        // this.bread = [];
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
}

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
  fileManger: any = [];
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

  constructor(
    protected fileManagerService: FileManagerService,
    private _FileSaverService: FileSaverService,
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
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotodetail(file): void {
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
    this.pagination(this.page.pageNumber, this.page.pageSize);
  }

  deleteFiles(): void {
    // let filesurl = `/files/${id}?driveName=Files`;
    // this.fileManagerService.DeleteFileFloder(this.fileID)
    this.httpService
      .delete(`/files/${this.fileID}?driveName=Files`)
      .subscribe(() => {
        // this.allFiles(5, 0);
        this.getByIdFile(this.bread[this.bread.length - 1].id);
      });
  }
  openRenameDialog(ref: TemplateRef<any>, file): void {
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
    formData.append('StoragePath', 'Files' + storagePath);
    formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    this.httpService
      .post(`/files?driveName=Files`, formData, { observe: 'response' })
      .subscribe((data) => {
        if (data && data.status === 200) {
          // if (this.filterOrderBy)
          //   this.pagination(
          //     this.page.pageNumber,
          //     this.page.pageSize,
          //     this.filterOrderBy
          //   );
          // else
          // console.log('length', this.bread.length);
          // console.log('bread', this.bread);
          // console.log('id', this.bread[this.bread.length]['id']);
          if (this.bread && !this.bread.length)
            this.pagination(1, this.page.pageSize);
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
    //   //console.log(data);
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
          this.native_file = output.file.nativeFile;
          this.native_file_name = output.file.nativeFile.name;
          // this.show_upload = false;
        }
        break;
    }
  }
  UploadFile(ref): void {
    let storagePath = '';
    this.bread.forEach((item) => (storagePath += '/' + item.name));
    let formData = new FormData();
    formData.append('Files', this.native_file, this.native_file_name);
    formData.append('StoragePath', 'Files' + storagePath);
    // formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    // this.fileManagerService.Createfolder(formData)
    // let createfile = `/files?driveName=Files`;
    this.httpService.post(`files?driveName=Files`, formData).subscribe(
      (data: any) => {
        //console.log(data);

        if (this.ChildFolderFlag == true) {
          this.getByIdFile(this.FolderIDs);
        } else if (this.ChildFolderFlag == false) {
          // this.allFiles(5, 0);
          this.pagination(this.page.pageNumber, this.page.pageSize);
        }
        ref.close();
      },
      (error) => {
        //console.log(error);
      }
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
  editFileName() {}

  onDown() {
    let fileName: string;
    this.fileManagerService
      .getFiledownload(this.fileID)
      // let downloadurl = `/files/${this.fileID}/download?driveName=Files`;
      // this.httpService
      //   .get(`/files/${this.fileID}/download?driveName=Files`)
      .subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this._FileSaverService.save(data.body, fileName);
      });
  }

  getFileId(val, i) {
    for (let abc in this.bread) {
      if (+abc > i) {
        this.bread.splice(+abc, this.bread.length - i);
        this.getByIdFile(this.bread[i].id);
      }
    }
  }

  onClickUp() {
    if (this.bread.length > 1) {
      this.bread.splice(this.bread.length - 1, 1);
      const id = this.bread[this.bread.length - 1].id;
      this.floderName = this.bread[this.bread.length - 1].name;
      this.getByIdFile(id);
    } else {
      this.bread.splice(this.bread.length - 1, 1);
      // this.allFiles(5, 0);
      this.pagination(this.page.pageNumber, this.page.pageSize);
      // this.ChildFolderFlag = false;
    }
  }

  getByIdFile(id) {
    // let filesurl = `/files?driveName=Files&$filter=${parentId}`;
    // this.fileManagerService
    //   .getFileFloder(`ParentId+eq+guid'${id}'`)
    this.httpService
      .get(`files?driveName=Files&$filter=ParentId+eq+guid'${id}'`)
      .subscribe((data: any) => {
        if (data && data.items) this.fileManger = [...data.items];
        this.page.totalCount = data.totalCount;

        // this.showpage = data;
        // this.bread = [];
        // this.gotodetail(this.fileManger[0]);// commented just 2 api calls
        // if (data.totalCount == 0) {
        //   this.get_perPage = false;
        // } else if (data.totalCount != 0) {
        //   this.get_perPage = true;
        // }
      });
  }

  fileFolder(files) {
    if (files && files.isFile == false) {
      this.ChildFolderFlag = true;
      this.floderName = files.name;
      this.bread.push(files);
      this.FolderIDs = files.id;
      this.getByIdFile(files.id);
    }
  }
  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.pagination(this.page.pageNumber, this.page.pageSize, `${param}+asc`);
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+desc`
      );
    }
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${this.filterOrderBy}`
      );
    } else this.pagination(this.page.pageNumber, this.page.pageSize);
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
      if (response && response.items.length) {
        this.bread = [];
        this.fileManger = [...response.items];
        this.page.totalCount = response.totalCount;
      } else this.fileManger = [];
    });
  }
  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}

// allFiles(top, skip) {
//   this.get_perPage = false;
//   this.fileManagerService.getAllFiles(top, skip).subscribe((data: any) => {
//     this.fileManger = data.items;
//     this.page.totalCount = data.totalCount;
//     this.showpage = data;
//     this.bread = [];
//     // this.gotodetail(this.fileManger[0]);
//     if (data.totalCount == 0) {
//       this.get_perPage = false;
//     } else if (data.totalCount != 0) {
//       this.get_perPage = true;
//     }
//   });
// }

// onSortClick(event, fil_val) {
//   let target = event.currentTarget,
//     classList = target.classList;
//   if (classList.contains('fa-chevron-up')) {
//     classList.remove('fa-chevron-up');
//     classList.add('fa-chevron-down');
//     let sort_set = 'desc';
//     this.sort(fil_val, sort_set);
//     this.sortDir = -1;
//   } else {
//     classList.add('fa-chevron-up');
//     classList.remove('fa-chevron-down');
//     let sort_set = 'asc';
//     this.sort(fil_val, sort_set);
//     this.sortDir = 1;
//   }
// }

// sort(filter_val, vale) {
//   const skip = (this.page.pageNumber - 1) * this.page.pageSize;
//   this.feild_name = filter_val + '+' + vale;
//   this.fileManagerService
//     .getAllFilesOrder(this.page.pageSize, skip, this.feild_name)
//     .subscribe((data: any) => {
//       this.showpage = data;
//       this.fileManger = data.items;
//     });
// }

// pageChanged(event) {
//   this.page.pageNumber = event;
//   this.pagination(event, this.page.pageSize);
// }

// pagination(pageNumber, pageSize?) {
//   if (this.show_perpage_size == false) {
//     const top: number = pageSize;
//     const skip = (pageNumber - 1) * pageSize;
//     if (this.feild_name.length == 0) {
//       this.allFiles(top, skip);
//       console.log('usman');
//     } else if (this.feild_name.length != 0) {
//       this.fileManagerService
//         .getAllFilesOrder(top, skip, this.feild_name)
//         .subscribe((data: any) => {
//           this.showpage = data;
//           this.fileManger = data.items;
//           this.page.totalCount = data.totalCount;
//         });
//     }
//   } else if (this.show_perpage_size == true) {
//     const top: number = this.per_page_num;
//     const skip = (pageNumber - 1) * this.per_page_num;
//     this.fileManagerService
//       .getAllFilesOrder(top, skip, this.feild_name)
//       .subscribe((data: any) => {
//         this.showpage = data;
//         this.fileManger = data.items;
//         this.page.totalCount = data.totalCount;
//       });
//   }
// }
// per_page(val) {
//   // let filesurl = `/files?driveName=Files&$orderby=createdOn+desc&$top=${tpage}&$skip=${spage}`;
//   this.per_page_num = val;
//   this.page.pageSize = val;
//   const skip = (this.page.pageNumber - 1) * this.per_page_num;
//   if (this.feild_name.length == 0) {
//     this.fileManagerService
//       .getAllFiles(this.page.pageSize, skip)
//       .subscribe((data: any) => {
//         this.showpage = data;
//         this.fileManger = data.items;
//         this.page.totalCount = data.totalCount;
//       });
//   } else if (this.feild_name.length != 0) {
//     this.show_perpage_size = true;
//     this.fileManagerService
//       .getAllFilesOrder(this.page.pageSize, skip, this.feild_name)
//       .subscribe((data: any) => {
//         this.showpage = data;
//         this.fileManger = data.items;
//         this.page.totalCount = data.totalCount;
//       });
//   }
// }

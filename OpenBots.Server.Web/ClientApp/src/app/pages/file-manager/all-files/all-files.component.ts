import { HttpResponse } from '@angular/common/http';
import { Component, EventEmitter, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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
  bread = [];
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
  //// select row ///
  HighlightRow: number;
  ClickedRow: any;

  constructor(
    protected fileManagerService: FileManagerService,
    private _FileSaverService: FileSaverService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder,
    private dialogService: DialogService,
    private helperService: HelperService
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

  allFiles(top, skip) {
    this.get_perPage = false;
    this.fileManagerService.getAllFiles(top, skip).subscribe((data: any) => {
      this.fileManger = data.items;
      this.page.totalCount = data.totalCount;
      this.showpage = data;
      this.bread = [];
      // this.gotodetail(this.fileManger[0]);
      if (data.totalCount == 0) {
        this.get_perPage = false;
      } else if (data.totalCount != 0) {
        this.get_perPage = true;
      }
    });
  }
  gotodetail(file) {
    if (file) {
      this.showDownloadbtn = true;
      this.fileID = file.id;
      this.name = file.name;
      this.size = file.size;
      this.contentType = file.contentType;
      this.createdOn = file.createdOn;
      this.fullStoragePath = file.fullStoragePath;
    }
  }

  openRenameDialog(ref: TemplateRef<any>, file): void {
    // if (file.isFile == true) {
    //   this.fileType = 'File';
    // } else {
    //   this.fileType = 'Folder';
    // }

    this.filesFormgroup = this.formBuilder.group({
      // , [Validators.pattern('/^[w.-]+$/')]
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
    // this.filesFormgroup.patchValue({ name: this.name });
    this.dialogService.openDialog(ref);
  }

  openpopupDialog(ref: TemplateRef<any>): void {
    this.dialogService.openDialog(ref);
  }
  createFolder(ref) {
    let storagePath = '';
    this.bread.forEach((item) => (storagePath += '\\' + item.name));
    let formData = new FormData();
    formData.append('Name', this.filesCreateFolderFromgroup.value.name);
    formData.append('StoragePath', 'Files' + storagePath);
    formData.append('isFile', this.filesCreateFolderFromgroup.value.isFile);
    this.fileManagerService.Createfolder(formData).subscribe(
      (data: any) => {
        console.log(data);
        this.allFiles(5, 0);
        ref.close();
      },
      (error) => {
        console.log(error);
      }
    );
  }
  get fc() {
    return this.filesCreateFolderFromgroup.controls;
  }
  get f() {
    return this.filesFormgroup.controls;
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
          // this.native_file = output.file.nativeFile;
          // this.native_file_name = output.file.nativeFile.name;
          // this.show_upload = false;
        }
        break;
    }
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
      this.bread.splice(this.bread.length, 1);
      this.allFiles(5, 0);
    }
  }

  getByIdFile(id) {
    this.fileManagerService
      .getFileFloder(`ParentId+eq+guid'${id}'`)
      .subscribe((data: any) => {
        this.fileManger = data.items;
        this.page.totalCount = data.totalCount;

        this.showpage = data;
        // this.bread = [];
        this.gotodetail(this.fileManger[0]);
        if (data.totalCount == 0) {
          this.get_perPage = false;
        } else if (data.totalCount != 0) {
          this.get_perPage = true;
        }
      });
  }

  fileFolder(files) {
    if (files && files.isFile == false) {
      this.floderName = files.name;
      this.bread.push(files);
      this.getByIdFile(files.id);
    }
  }

  onSortClick(event, fil_val) {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      let sort_set = 'desc';
      this.sort(fil_val, sort_set);
      this.sortDir = -1;
    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      let sort_set = 'asc';
      this.sort(fil_val, sort_set);
      this.sortDir = 1;
    }
  }

  sort(filter_val, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_val + '+' + vale;
    this.fileManagerService
      .getAllFilesOrder(this.page.pageSize, skip, this.feild_name)
      .subscribe((data: any) => {
        this.showpage = data;
        this.fileManger = data.items;
      });
  }
  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  per_page(val) {
    this.per_page_num = val;
    this.page.pageSize = val;
    const skip = (this.page.pageNumber - 1) * this.per_page_num;
    if (this.feild_name.length == 0) {
      this.fileManagerService
        .getAllFiles(this.page.pageSize, skip)
        .subscribe((data: any) => {
          this.showpage = data;
          this.fileManger = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (this.feild_name.length != 0) {
      this.show_perpage_size = true;
      this.fileManagerService
        .getAllFilesOrder(this.page.pageSize, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.fileManger = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  pagination(pageNumber, pageSize?) {
    if (this.show_perpage_size == false) {
      const top: number = pageSize;
      const skip = (pageNumber - 1) * pageSize;
      if (this.feild_name.length == 0) {
        this.allFiles(top, skip);
      } else if (this.feild_name.lenght != 0) {
        this.fileManagerService
          .getAllFilesOrder(top, skip, this.feild_name)
          .subscribe((data: any) => {
            this.showpage = data;
            this.fileManger = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.show_perpage_size == true) {
      const top: number = this.per_page_num;
      const skip = (pageNumber - 1) * this.per_page_num;
      this.fileManagerService
        .getAllFilesOrder(top, skip, this.feild_name)
        .subscribe((data: any) => {
          this.showpage = data;
          this.fileManger = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
}

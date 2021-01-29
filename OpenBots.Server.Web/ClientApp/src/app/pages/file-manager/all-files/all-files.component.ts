import { HttpResponse } from '@angular/common/http';
import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { FileSaverService } from 'ngx-filesaver';
import { DialogService } from '../../../@core/dialogservices';
import { FileManagerService } from '../fileManager.service';
import { HelperService } from '../../../@core/services/helper.service';
import { Page } from '../../../interfaces/paginateInstance';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
@Component({
  selector: 'ngx-all-files',
  templateUrl: './all-files.component.html',
  styleUrls: ['./all-files.component.scss'],
})
export class AllFilesComponent implements OnInit {
  filesFormgroup: FormGroup;
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

  ///////

  showpage: any = [];
  sortDir = 1;
  view_dialog: any;
  showData;
  del_id: any = [];
  toggle: boolean;
  feild_name: any = [];
  page: Page = {};
  get_perPage: boolean = false;
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected fileManagerService: FileManagerService,
    private _FileSaverService: FileSaverService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder,
    private dialogService: DialogService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  allFiles(top, skip) {
    this.get_perPage = false;
    // this.emailService.getAllEmail(top, skip).subscribe((data: any) => {
    this.fileManagerService.getAllFiles(top, skip).subscribe((data: any) => {
      this.fileManger = data.items;
          this.page.totalCount = data.totalCount;
            this.showpage = data;
      this.bread = [];
      this.gotodetail(this.fileManger[0]);
       if (data.totalCount == 0) {
         this.get_perPage = false;
       } else if (data.totalCount != 0) {
         this.get_perPage = true;
       }
    });
  }
  gotodetail(file) {
    this.fileID = file.id;
    this.name = file.name;
    this.size = file.size;
    this.contentType = file.contentType;
    this.createdOn = file.createdOn;
    this.fullStoragePath = file.fullStoragePath;
  }

  openDeleteDialog(ref: TemplateRef<any>, file): void {
    if (file.isFile == true) {
      this.fileType = 'File';
    } else {
      this.fileType = 'Folder';
    }

    this.filesFormgroup = this.formBuilder.group({
      // , [Validators.pattern('/^[w.-]+$/')]
      name: [''],
    });
    this.filesFormgroup.patchValue({ name: file.name });
    this.dialogService.openDialog(ref);
  }

  get f() {
    return this.filesFormgroup.controls;
  }

  editFileName(file) {
    console.log(file);
    this.filesFormgroup = this.formBuilder.group({
      name: [''],
    });
    this.filesFormgroup.patchValue({ name: file.name });
  }

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
    console.log(val);
    console.log(i);
    console.log(this.bread);
    for (let abc in this.bread) {
      if (+abc > i) {
        this.bread.splice(+abc, this.bread.length - i);
      }
    }
  }

  fileFolder(files, i) {
    this.floderName = files.name;
    // this.bread.push(files);
    // console.log(this.bread);
    if (files.isFile == false && files.hasChild == true) {
      console.log(i);
      this.bread.push(files);

      this.fileManagerService.getFileFloder(files.id).subscribe((data: any) => {
        this.fileManger = data.items;
      });
    } else if (files.isFile == false && files.hasChild == false) {
      this.toastrService.info(
        `does not contain any sub folder or files`,
        ` ${files.name}`
      );
    }
  }
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

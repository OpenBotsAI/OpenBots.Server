import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { DialogService } from '../../../@core/dialogservices';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { Page } from '../../../interfaces/paginateInstance';
import { AssetService } from '../asset.service';

@Component({
  selector: 'ngx-all-asset',
  templateUrl: './all-asset.component.html',
  styleUrls: ['./all-asset.component.scss'],
})
export class AllAssetComponent implements OnInit {
  isDeleted = false;
  showpage: any = [];
  showallassets: any = [];
  sortDir = 1;
  viewDialog: any;
  showData;
  delId: any = [];
  toggle: boolean;
  feildName: any = [];
  page: Page = {};
  getPerPage: boolean = false;
  showPerpageSize: boolean = false;
  perPageNum: any = [];
  itemsPerPage: ItemsPerPage[] = [];
  searchedValue: string;
  filterOrderBy: string;
  constructor(
    protected router: Router,
    private dialogService: DialogService,
    protected assestService: AssetService,
    private helperService: HelperService,
    private toastrService: NbToastrService
  ) {}

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  gotoadd() {
    this.router.navigate(['/pages/asset/add']);
  }
  gotoedit(id) {
    this.router.navigate([`/pages/asset/edit/${id}`]);
  }
  gotodetail(id) {
    this.router.navigate(['/pages/asset/get-asset-id'], {
      queryParams: { id: id },
    });
  }

  sort(filter_value, vale) {
    // const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    // this.feildName = filter_val + '+' + vale;
    // this.assestService
    //   .getAllAssetOrder(this.page.pageSize, skip, this.feildName)
    //   .subscribe((data: any) => {
    //     this.showpage = data;
    //     this.showallassets = data.items;
    //   });
    const top = this.page.pageSize;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.filterOrderBy = `${filter_value}+${vale}`;
    if (this.searchedValue) {
      if (this.filterOrderBy) {
        this.assestService
          .getFilterPagination(
            top,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.assestService
          .getFilterPagination(top, skip, 'createdOn+desc', this.searchedValue)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (this.filterOrderBy) {
      this.assestService
        .getFilterPagination(top, skip, this.filterOrderBy)
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallassets = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else {
      this.assestService
        .getFilterPagination(top, skip, 'createdOn+desc')
        .subscribe((data: any) => {
          this.showpage = data;
          this.showallassets = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  open2(dialog: TemplateRef<any>, id: any) {
    this.delId = [];
    this.viewDialog = dialog;
    this.dialogService.openDialog(dialog);
    this.delId = id;
  }

  del_agent(ref) {
    this.isDeleted = true;
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.assestService.delAssetbyID(this.delId).subscribe(
      () => {
        this.isDeleted = false;
        this.toastrService.success('Deleted Successfully');
        ref.close();
        // this.get_allasset(this.page.pageSize, skip);
         this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      () => (this.isDeleted = false)
    );
  }

  get_allasset(top, skip) {
    this.getPerPage = false;
    this.assestService.getAllAsset(top, skip).subscribe((data: any) => {
      for (const item of data.items) {
        if (item.valueJson) {
          item.valueJson = JSON.stringify(item.valueJson);
          item.valueJson = JSON.parse(item.valueJson);
        }
      }
      this.showpage = data;
      this.showallassets = data.items;

      this.page.totalCount = data.totalCount;
      this.getPerPage = true;
    });
  }

  cleanString(str) {
    str = str.replace('"{', '{');
    str = str.replace('}"', '}');
    return str;
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

  per_page(val) {
    // this.perPageNum = val;
    // this.page.pageSize = val;
    // const skip = (this.page.pageNumber - 1) * this.perPageNum;
    // if (this.feildName.length == 0) {
    //   this.assestService
    //     .getAllAsset(this.page.pageSize, skip)
    //     .subscribe((data: any) => {
    //       this.showpage = data;
    //       this.showallassets = data.items;
    //       this.page.totalCount = data.totalCount;
    //     });
    // } else if (this.feildName.length != 0) {
    //   this.showPerpageSize = true;
    //   this.assestService
    //     .getAllAssetOrder(this.page.pageSize, skip, this.feildName)
    //     .subscribe((data: any) => {
    //       this.showpage = data;
    //       this.showallassets = data.items;
    //       this.page.totalCount = data.totalCount;
    //     });
    // }
     this.perPageNum = val;
     this.showPerpageSize = true;
     this.page.pageSize = val;
     const skip = (this.page.pageNumber - 1) * this.perPageNum;
     if (this.searchedValue) {
       if (this.filterOrderBy) {
         this.assestService
           .getFilterPagination(
             this.page.pageSize,
             skip,
             this.filterOrderBy,
             this.searchedValue
           )
           .subscribe((data: any) => {
             this.showpage = data;
             this.showallassets = data.items;
             this.page.totalCount = data.totalCount;
           });
       } else {
         this.assestService
           .getFilterPagination(
             this.page.pageSize,
             skip,
             'createdOn+desc',
             this.searchedValue
           )
           .subscribe((data: any) => {
             this.showpage = data;
             this.showallassets = data.items;
             this.page.totalCount = data.totalCount;
           });
       }
     } else if (this.filterOrderBy) {
       this.assestService
         .getFilterPagination(this.page.pageSize, skip, this.filterOrderBy)
         .subscribe((data: any) => {
           this.showpage = data;
           this.showallassets = data.items;
           this.page.totalCount = data.totalCount;
         });
     } else {
       this.assestService
         .getFilterPagination(this.page.pageSize, skip, 'createdOn+desc')
         .subscribe((data: any) => {
           this.showpage = data;
           this.showallassets = data.items;
           this.page.totalCount = data.totalCount;
         });
     }

  }

  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize?) {
    // if (this.showPerpageSize == false) {
    //   const top: number = pageSize;
    //   const skip = (pageNumber - 1) * pageSize;
    //   if (this.feildName.length == 0) {
    //     this.get_allasset(top, skip);
    //   } else if (this.feildName.lenght != 0) {
    //     this.assestService
    //       .getAllAssetOrder(top, skip, this.feildName)
    //       .subscribe((data: any) => {
    //         this.showpage = data;
    //         this.showallassets = data.items;
    //         this.page.totalCount = data.totalCount;
    //       });
    //   }
    // } else if (this.showPerpageSize == true) {
    //   const top: number = this.perPageNum;
    //   const skip = (pageNumber - 1) * this.perPageNum;
    //   this.assestService
    //     .getAllAssetOrder(top, skip, this.feildName)
    //     .subscribe((data: any) => {
    //       this.showpage = data;
    //       this.showallassets = data.items;
    //       this.page.totalCount = data.totalCount;
    //     });
    // }
     const top = pageSize;
     this.page.pageSize = pageSize;
     const skip = (pageNumber - 1) * pageSize;
     if (this.searchedValue) {
       if (this.filterOrderBy) {
         this.assestService
           .getFilterPagination(
             top,
             skip,
             this.filterOrderBy,
             this.searchedValue
           )
           .subscribe((data: any) => {
             this.showpage = data;
             this.showallassets = data.items;
             this.page.totalCount = data.totalCount;
           });
       } else {
         this.assestService
           .getFilterPagination(top, skip, 'createdOn+desc', this.searchedValue)
           .subscribe((data: any) => {
             this.showpage = data;
             this.showallassets = data.items;
             this.page.totalCount = data.totalCount;
           });
       }
     } else if (this.filterOrderBy) {
       this.assestService
         .getFilterPagination(top, skip, this.filterOrderBy)
         .subscribe((data: any) => {
           this.showpage = data;
           this.showallassets = data.items;
           this.page.totalCount = data.totalCount;
         });
     } else {
       this.assestService
         .getFilterPagination(top, skip, 'createdOn+desc')
         .subscribe((data: any) => {
           this.showpage = data;
           this.showallassets = data.items;
           this.page.totalCount = data.totalCount;
         });
     }
  }

  trackByFn(index: number, item: unknown): number {
    if (!item) return null;
    return index;
  }
  searchValue(event) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    if (event.target.value.length >= 2) {
      this.searchedValue = event.target.value;
      if (this.filterOrderBy) {
        this.assestService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            this.filterOrderBy,
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else {
        this.assestService
          .getFilterPagination(
            this.page.pageSize,
            skip,
            'createdOn+desc',
            this.searchedValue
          )
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
      }
    } else if (!event.target.value.length) {
      this.searchedValue = null;
      if (this.filterOrderBy) {
        this.assestService
          .getFilterPagination(this.page.pageSize, skip, this.filterOrderBy)
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
      } else
        this.assestService
          .getFilterPagination(this.page.pageSize, skip, 'createdOn+desc')
          .subscribe((data: any) => {
            this.showpage = data;
            this.showallassets = data.items;
            this.page.totalCount = data.totalCount;
          });
    }
  }
}

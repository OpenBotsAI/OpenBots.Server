import { DatePipe } from '@angular/common';
import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { TimeDatePipe } from '../../../@core/pipe';
import { ConfigValueService } from '../config-value.service';

@Component({
  selector: 'ngx-get-config-value',
  templateUrl: './get-config-value.component.html',
  styleUrls: ['./get-config-value.component.scss']
})
export class GetConfigValueComponent implements OnInit {


  showconfigValue: any = [];
  configform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];
  etag;
  configId;
  constructor(
    private acroute: ActivatedRoute,
    protected configService: ConfigValueService,
    private toastrService: NbToastrService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.configId = params.id
      this.getConfigValueById(params.id);
    });
  }

  ngOnInit(): void {
    this.configform = this.formBuilder.group({
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      description: [''],
      id: [''],
      isDeleted: [''],
      name: [''],
      timestamp: [''],
      uiHint: [''],
      updatedBy: [''],
      updatedOn: [''],
      validationRegex: [''],
      value: [''],
    });
  }





  getConfigValueById(id) {
    this.configService.getConfigValuebyId(id).subscribe((data: HttpResponse<any>) => {
      this.showconfigValue = data.body;
      this.etag = data.headers.get('ETag').replace(/\"/g, '')
      console.log(this.etag)
      const filterPipe = new TimeDatePipe();
      this.showconfigValue.updatedOn = filterPipe.transform(this.showconfigValue.updatedOn, 'lll');
      this.configform.patchValue(this.showconfigValue);
      this.configform.disable();

    }, (error) => {
      console.log(error.status, error)
      if (error.error.status === 409) {
        this.toastrService.danger(error.error.serviceErrors, 'error')
        this.getConfigValueById(this.configId)
      }
    });
  }



  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'Configuration.ConfigurationValue', id: this.showconfigValue.id } })
  }


}

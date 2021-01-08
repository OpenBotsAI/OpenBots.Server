import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AutomationService } from '../automation.service';
import { DatePipe } from '@angular/common';
import { FormGroup, FormBuilder } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';
import { FileSaverService } from 'ngx-filesaver';
import {
  HttpRequest,
  HttpResponse,
  HttpResponseBase,
} from '@angular/common/http';
import { first } from 'rxjs/operators';

@Component({
  selector: 'ngx-get-automation-id',
  templateUrl: './get-automation-id.component.html',
  styleUrls: ['./get-automation-id.component.scss'],
})
export class GetAutomationIdComponent implements OnInit {
  process_id: any = [];
  jsonValue: any = [];
  show_allprocess: any = [];
  showprocess: FormGroup;
  show_createdon: any = [];
  automationSelection: string[] = ['OpenBots', 'Python'];
  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    private _FileSaverService: FileSaverService,
    protected automationService: AutomationService,
    protected router: Router
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.process_id = params.id;
      this.get_process(params.id);
    });
  }

  ngOnInit(): void {
    this.showprocess = this.formBuilder.group({
      binaryObjectId: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      name: [''],
      status: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
      version: [''],
      automationEngine: [''],
    });
  }

  get_process(id) {
    this.automationService
      .getProcessId(id)
      .subscribe((data: HttpResponse<any>) => {
        this.show_allprocess = data.body;
        const filterPipe = new TimeDatePipe();
        console.log(data.headers.get('ETag').replace(/\"/g, ''));
        data.body.createdOn = filterPipe.transform(data.body.createdOn, 'lll');
        this.showprocess.patchValue(data.body);
        this.showprocess.disable();
      });
  }

  onDown() {
    let fileName: string;
    this.automationService
      .getBlob(this.process_id)
      .subscribe((data: HttpResponse<Blob>) => {
        fileName = data.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this._FileSaverService.save(data.body, fileName);
      });
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        // PageName: 'Process',
        PageName: 'Automation',

        id: this.show_allprocess.id,
      },
    });
  }
}

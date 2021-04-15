import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusinessEventService } from '../business-event.service';
import { TimeDatePipe } from '../../../@core/pipe';
// import { JsonEditorOptions } from 'ang-jsoneditor';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { NbToastrService } from '@nebular/theme';
@Component({
  selector: 'ngx-view-business-event',
  templateUrl: './view-business-event.component.html',
  styleUrls: ['./view-business-event.component.scss']
})
export class ViewBusinessEventComponent implements OnInit {

  submitted = false;
  createdOn: any = [];
  showallsystemEvent: any = [];
  payloadSchema: any = [];
  businessEventform: FormGroup;
  showChangedToJson: boolean = false;
  pipe: TimeDatePipe;
  UrlID;
  public editorOptions: JsonEditorOptions;
  @ViewChild('editor') editor: JsonEditorComponent;

  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected BusinessEventservice: BusinessEventService,
  ) {
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];

    //this.editorOptions.onChange = () =>
    this.UrlID = this.acroute.snapshot.params['id']
    this.getBusinessEventID(this.UrlID)
    // this.acroute.queryParams.subscribe((params) => {
    //   this.get_allagent(params.id);
    // });
  }

  ngOnInit(): void {
    this.businessEventform = this.formBuilder.group({
      name: [''],
      description: [''],
      entityType: [''],
      payloadSchema: ['']
    });
  }




  getBusinessEventID(id) {
    this.BusinessEventservice.getSystemEventid(id).subscribe((data: any) => {
      this.showallsystemEvent = data;
      data.createdOn = this.transformDate(data.createdOn, 'lll');
      this.businessEventform.patchValue(data);
      this.businessEventform.disable();

      // if (data.payloadSchema != null) {
      //   this.showpayloadSchemaJson = true;
      //   this.payloadSchema = data.payloadSchema;
      //   this.payloadSchema = JSON.parse(this.payloadSchema);
      // }

    });
  }

  transformDate(value, format) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }
}

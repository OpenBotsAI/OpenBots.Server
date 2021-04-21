import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusinessEventService } from '../business-event.service';
import { NbToastrService } from '@nebular/theme';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';

@Component({
  selector: 'ngx-raise-business-event',
  templateUrl: './raise-business-event.component.html',
  styleUrls: ['./raise-business-event.component.scss']
})
export class RaiseBusinessEventComponent implements OnInit {
  public editorOptions: JsonEditorOptions;
  @ViewChild('editor') editor: JsonEditorComponent;
  submitted = false;
  createdOn: any = [];
  showallsystemEvent: any = [];
  payloadSchema: any = [];
  raiseBusinessEventform: FormGroup;
  showChangedToJson: boolean = false;
  show_Entityname: any = [];
  title = 'Add';


  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected BusinessEventservice: BusinessEventService,
  ) {
    this.editorOptions = new JsonEditorOptions();
    this.editorOptions.modes = ['code', 'text', 'tree', 'view'];
    this.entityName();

  }

  ngOnInit(): void {
    this.raiseBusinessEventform = this.formBuilder.group({
      entityName: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100),
        Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
      ]],
      payloadSchema: [''],
      message: [''],
      page_name: [''],
      entityId: ['', [Validators.required, Validators.pattern('^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$')]],
    });


  }
  entityName() {
    this.BusinessEventservice.getBusinessEventName().subscribe((data: any) => {
      this.show_Entityname = data.items;
    });
  }

  get f() {
    return this.raiseBusinessEventform.controls;
  }

  getEntityname(e) {
    console.log(e)
    console.log(this.raiseBusinessEventform.value.page_name)
  }

  onSubmit() {
    this.submitted = true;

    this.BusinessEventservice.raiseBusinessEvent(this.raiseBusinessEventform.value,
      this.raiseBusinessEventform.value.entityId).subscribe(
        () => {
          this.toastrService.success(
            'Raise Business Event Save Successfully!',
            'Success'
          );
          this.router.navigate(['pages/business-event/list']);
        },
        (error) => {
          this.submitted = false;

        }
      )

  }

}

import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusinessEventService } from '../business-event.service';
import { NbToastrService } from '@nebular/theme';

@Component({
  selector: 'ngx-raise-business-event',
  templateUrl: './raise-business-event.component.html',
  styleUrls: ['./raise-business-event.component.scss']
})
export class RaiseBusinessEventComponent implements OnInit {

  submitted = false;
  createdOn: any = [];
  showallsystemEvent: any = [];
  payloadSchema: any = [];
  raiseBusinessEventform: FormGroup;
  showChangedToJson: boolean = false;

  title = 'Add';


  constructor(
    private acroute: ActivatedRoute,
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected BusinessEventservice: BusinessEventService,
  ) {


  }

  ngOnInit(): void {
    this.raiseBusinessEventform = this.formBuilder.group({
      entityName: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100),
        Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
      ]],
      entityId: ['', [Validators.required, Validators.pattern('^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$')]],
    });

  }

  get f() {
    return this.raiseBusinessEventform.controls;
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

import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbDateService, NbToastrService } from '@nebular/theme';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-add-email-account',
  templateUrl: './add-email-account.component.html',
  styleUrls: ['./add-email-account.component.scss']
})
export class AddEmailAccountComponent implements OnInit {
  min: Date;
  max: Date;
  emailId: any = [];
  submitted = false;
  showEmail: any = [];
  emailform: FormGroup;
  pipe = new DatePipe('en-US');
  now = Date();
  show_createdon: any = [];

  constructor(
    private toastrService: NbToastrService, private dateService: NbDateService<Date>,
    protected emailService: EmailAccountsService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {

  }

  ngOnInit(): void {

    this.emailform = this.formBuilder.group({
      fromEmailAddress: ['', [Validators.required, Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$')]],
      fromName: [''],
      host: [''],
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$')]],
      passwordHash: [''],
      port: [''],
      provider: ['', [Validators.required]],
      username: [''],
    });
    this.min = new Date();
    this.max = new Date();
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }


  get f() {
    return this.emailform.controls;
  }





  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.email', id: this.showEmail.id } })
  }



  onSubmit() {
    this.submitted = true;
    this.emailService
      .addEmail(this.emailform.value)
      .subscribe(() => {
        this.toastrService.success('Email account created successfully', 'Success');
        this.router.navigate(['pages/emailaccount/list']);
        this.submitted = false
      }, () => this.submitted = false);
  }
}
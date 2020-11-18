import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbDateService, NbToastrService } from '@nebular/theme';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-email-testing-account',
  templateUrl: './email-testing-account.component.html',
  styleUrls: ['./email-testing-account.component.scss']
})
export class EmailTestingAccountComponent implements OnInit {

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
      address: ['', [Validators.required, Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$')]],
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$')]],
      subject: [''],
      body: [''],

    });

  }


  get f() {
    return this.emailform.controls;
  }





  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.email', id: this.showEmail.id } })
  }



  onSubmit() {
    console.log(this.emailform.value)
    // this.submitted = true;
    // this.emailService
    //   .addEmail(this.emailform.value)
    //   .subscribe(() => {
    //     this.toastrService.success('Email account created successfully', 'Success');
    //     this.router.navigate(['pages/emailaccount/list']);
    //     this.submitted = false
    //   }, () => this.submitted = false);
  }
}
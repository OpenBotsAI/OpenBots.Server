import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { EmailAccountsService } from '../email-accounts.service';

@Component({
  selector: 'ngx-email-testing-account',
  templateUrl: './email-testing-account.component.html',
  styleUrls: ['./email-testing-account.component.scss']
})
export class EmailTestingAccountComponent implements OnInit {
  @ViewChild("myckeditor") ckeditor: any;
  submitted = false;
  showEmail: any = [];
  emailform: FormGroup;
  ckeConfig: any;
  queryParamName: string;
  queryParamEmail: string;

  constructor(
    private toastrService: NbToastrService, private route: ActivatedRoute,
    protected emailService: EmailAccountsService,
    private formBuilder: FormBuilder, protected router: Router,
  ) {
    this.ckeConfig = {
      allowedContent: false,
      extraPlugins: "divarea",
      forcePasteAsPlainText: true,
      removePlugins: 'about',
      // removePlugins: 'horizontalrule,tabletools,specialchar,about,list,others',
      // removeButtons: 'Save,NewPage,Preview,Print,Templates,Replace,SelectAll,Form,Checkbox,Radio,TextField,Textarea,Find,Select,Button,ImageButton,HiddenField,JustifyLeft,JustifyCenter,JustifyRight,JustifyBlock,CopyFormatting,CreateDiv,BidiLtr,BidiRtl,Language,Flash,Smiley,PageBreak,Iframe,Font,FontSize,TextColor,BGColor,ShowBlocks,Cut,Copy,Paste,Table,Image,Format,Source,Maximize,Styles,Anchor,SpecialChar,PasteFromWord,PasteText,Scayt,Undo,Redo,Strike,RemoveFormat,Indent,Outdent,Blockquote,Underline'
      removeButtons: 'Save,NewPage,Print,Preview'
    };
    this.route.queryParams.subscribe((params) => {

      this.queryParamName = params.name
      this.queryParamEmail = params.email

    });
  }

  ngOnInit(): void {

    this.emailform = this.formBuilder.group({
      address: ['', [Validators.required, Validators.pattern('^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$')]],
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$')]],
      subject: ['', [Validators.required]],
      body: [''],

    });

    this.emailform.patchValue({
      name: this.queryParamName,
      address: this.queryParamEmail
    }
    )


  }
  onChange($event: any): void {
    console.log("onChange");

  }

  onPaste($event: any): void {
    console.log("onPaste");
  }

  get f() {
    return this.emailform.controls;
  }





  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.email', id: this.showEmail.id } })
  }



  onSubmit() {
    this.submitted = true;
    let obj =
    {
      to: [
        {
          name: this.emailform.value.name,
          address: this.emailform.value.address,
        }
      ],
      subject: this.emailform.value.subject,
      body: this.emailform.value.body,
      isBodyHtml: true
    }

    this.emailService
      .testEmail(this.emailform.value.name, obj)
      .subscribe(() => {
        this.toastrService.success('Email test successfully.', 'Success');
        this.router.navigate(['pages/emailaccount/list']);
        this.submitted = false
      }, () => this.submitted = false);
  }
}
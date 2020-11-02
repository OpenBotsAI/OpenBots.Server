import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { Router, ActivatedRoute } from '@angular/router';
import { NbDateService } from '@nebular/theme';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-add-credentials',
  templateUrl: './add-credentials.component.html',
  styleUrls: ['./add-credentials.component.scss'],
})
export class AddCredentialsComponent implements OnInit {
  credentialForm: FormGroup;
  currentUrlId: string;
  min: Date;
  max: Date;
  title = 'Add';
  isSubmitted = false;
  trimError: boolean;
  providerArr = [
    { id: 'AD', name: 'Active Directory' },
    { id: 'A', name: 'Application' },
  ];
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute,
    private dateService: NbDateService<Date>,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.min = new Date();
    this.max = new Date();
    this.currentUrlId = this.route.snapshot.params['id'];
    this.credentialForm = this.initializeForm();
    if (this.currentUrlId) {
      this.title = 'Update';
      this.getCredentialsById();
    }
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }

  initializeForm() {
    return this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      provider: ['', [Validators.required]],
      domain: ['', [Validators.minLength(3), Validators.maxLength(100)]],
      userName: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      passwordSecret: [
        '',
        [Validators.minLength(3), Validators.maxLength(100)],
      ],
      startDate: [''],
      endDate: [''],
    });
  }

  get controls() {
    return this.credentialForm.controls;
  }

  credential(): void {
    if (this.credentialForm.valid) {
      this.isSubmitted = true;
      if (this.currentUrlId) this.updateCredentials();
      else this.addCredentials();
    }
  }
  addCredentials(): void {
    if (this.credentialForm.value.startDate) {
      this.credentialForm.value.startDate = this.helperService.transformDate(
        this.credentialForm.value.startDate,
        'lll'
      );
    }
    if (this.credentialForm.value.endDate) {
      this.credentialForm.value.endDate = this.helperService.transformDate(
        this.credentialForm.value.endDate,
        'lll'
      );
    }
    this.httpService
      .post('Credentials', this.credentialForm.value, { observe: 'response' })
      .subscribe(
        (response) => {
          if (response && response.status == 201) {
            this.httpService.success('Credential created successfully');
            this.isSubmitted = false;
            this.credentialForm.reset();
            this.router.navigate(['/pages/credentials']);
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  updateCredentials(): void {
    if (this.credentialForm.value.startDate) {
      this.credentialForm.value.startDate = this.helperService.transformDate(
        this.credentialForm.value.startDate,
        'lll'
      );
    }
    if (this.credentialForm.value.endDate) {
      this.credentialForm.value.endDate = this.helperService.transformDate(
        this.credentialForm.value.endDate,
        'lll'
      );
    }
    this.httpService
      .put(`Credentials/${this.currentUrlId}`, this.credentialForm.value, {
        observe: 'response',
      })
      .subscribe(
        (response) => {
          if (response) {
            this.httpService.success('Credential updated successfully');
            this.isSubmitted = false;
            this.credentialForm.reset();
            this.router.navigate(['/pages/credentials']);
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  getCredentialsById(): void {
    this.httpService
      .get(`Credentials/View/${this.currentUrlId}`)
      .subscribe((response: any) => {
        if (response) {
          this.min = response.startDate;
          this.credentialForm.patchValue({ ...response });
        }
      });
  }
}

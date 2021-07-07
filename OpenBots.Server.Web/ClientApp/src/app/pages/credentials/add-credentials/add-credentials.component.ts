import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { Router, ActivatedRoute } from '@angular/router';
import { NbDateService } from '@nebular/theme';
import { HelperService } from '../../../@core/services/helper.service';
import { AgentApiUrl, CredentialsApiUrl } from '../../../webApiUrls';
import { DialogService } from '../../../@core/dialogservices';

@Component({
  selector: 'ngx-add-credentials',
  templateUrl: './add-credentials.component.html',
  styleUrls: ['./add-credentials.component.scss'],
})
export class AddCredentialsComponent implements OnInit {
  credentialForm: FormGroup;
  agentCredentialForm: FormGroup;
  showLookUpagent: any = [];
  showCredAsstData: any = [];
  showGlobalAsset: boolean = false;
  showAgentAssetBtn: boolean = true;
  hideAgentAssetBtn: boolean = false;
  showUpdateAssetAgentbutton: boolean = false;
  showSaveAssetAgentbutton: boolean = true;
  getCredAgent: any;
  getAgentD: any;
  currentUrlId: string;
  min: Date;
  max: Date;
  title = 'Add';
  isSubmitted = false;
  trimError: boolean;
  eTag: string;
  isDeleted = false;
  deleteId: string;
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
    private helperService: HelperService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.min = new Date();
    this.max = new Date();
    this.currentUrlId = this.route.snapshot.params['id'];
    this.credentialForm = this.initializeForm();
    this.agentCredentialForm = this.initializeAgentCredentialForm();
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
      domain: [
        '',
        [
          Validators.minLength(3),
          Validators.maxLength(67),
          Validators.pattern(
            '^([A-Za-z0-9]{1,63}(-[A-Za-z0-9]{1,63})*?(\\.[A-Za-z0-9]{2,3})?)$'
          ),
        ],
      ],
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

  initializeAgentCredentialForm() {
    return this.fb.group({
      name: [''],
      agentId: [''],
      domain: [
        '',
        [
          Validators.minLength(3),
          Validators.maxLength(67),
          Validators.pattern(
            '^([A-Za-z0-9]{1,63}(-[A-Za-z0-9]{1,63})*?(\\.[A-Za-z0-9]{2,3})?)$'
          ),
        ],
      ],
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

  get agentCredentialcontrols() {
    return this.agentCredentialForm.controls;
  }

  //// start code of add agent creditional///

  showAssetAgentBox() {
    this.showGlobalAsset = true;
    this.hideAgentAssetBtn = true;
    this.showAgentAssetBtn = false;

    this.httpService
      .get(`${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`)
      .subscribe((response) => {
        this.showLookUpagent = response;
      });
    this.httpService
      .get(
        `${CredentialsApiUrl.credentials}/${CredentialsApiUrl.view}?$filter=name+eq+'${this.credentialForm.value.name}'and agentId+ne+null`
      )
      .subscribe((response) => {
        this.showCredAsstData = response.items;
        console.log(this.showCredAsstData);
      });
  }

  hideAssetAgentBox() {
    this.showGlobalAsset = false;
    this.hideAgentAssetBtn = false;
    this.showAgentAssetBtn = true;
  }

  editAssetAgent(credAgentValue) {
    this.getCredAgent = credAgentValue.id;
    this.getAgentD = credAgentValue.agentId;
    this.agentCredentialForm.get('agentId').disable();
    this.agentCredentialForm.patchValue({ ...credAgentValue });
    this.agentCredentialForm.get('passwordSecret').setValue(null);
    this.showUpdateAssetAgentbutton = true;
    this.showSaveAssetAgentbutton = false;
  }

  UpdateCredAgent() {
    if (!this.agentCredentialForm.get('passwordSecret').touched)
      this.agentCredentialForm.get('passwordSecret').setValue(null);
    else if (
      this.agentCredentialForm.get('passwordSecret').touched &&
      !this.agentCredentialForm.value.passwordSecret
    )
      this.agentCredentialForm.get('passwordSecret').setValue('');
    const headers = this.helperService.getETagHeaders(this.eTag);
    if (this.agentCredentialForm.value.startDate) {
      this.agentCredentialForm.value.startDate =
        this.helperService.transformDate(
          this.agentCredentialForm.value.startDate,
          'lll'
        );
    }
    if (this.agentCredentialForm.value.endDate) {
      this.agentCredentialForm.value.endDate = this.helperService.transformDate(
        this.agentCredentialForm.value.endDate,
        'lll'
      );
    }
    this.agentCredentialForm.value.agentId = this.getAgentD;
    this.httpService
      .put(
        `${CredentialsApiUrl.credentials}/${this.getCredAgent}`,
        this.agentCredentialForm.value,
        {
          observe: 'response',
          headers,
        }
      )
      .subscribe(
        (response) => {
          if (response) {
            this.httpService.success('Credential updated successfully');
            this.isSubmitted = false;
          }
        },
        (error) => {
          if (error && error.error && error.error.status === 409) {
            this.isSubmitted = false;
            this.httpService.error(error.error.serviceErrors);
            this.getCredentialsById();
          }
        }
      );
  }

  SaveCredAsset(): void {
    console.log(this.agentCredentialForm.value);
    if (this.agentCredentialForm.value.startDate) {
      this.agentCredentialForm.value.startDate =
        this.helperService.transformDate(
          this.agentCredentialForm.value.startDate,
          'lll'
        );
    }
    if (this.agentCredentialForm.value.endDate) {
      this.agentCredentialForm.value.endDate = this.helperService.transformDate(
        this.agentCredentialForm.value.endDate,
        'lll'
      );
    }
    this.agentCredentialForm.value.name = this.credentialForm.value.name;
    this.httpService
      .post(
        `${CredentialsApiUrl.credentials}/${CredentialsApiUrl.AddAgentCredential}`,
        this.agentCredentialForm.value,
        {
          observe: 'response',
        }
      )
      .subscribe(
        (response) => {
          if (response && response.status == 201) {
            this.httpService.success('Credential created successfully');
            this.isSubmitted = false;
            this.agentCredentialForm.reset();
            this.httpService
              .get(
                `${CredentialsApiUrl.credentials}?$filter=name+eq+'${this.credentialForm.value.name}'and agentId+ne+null`
              )
              .subscribe((response) => {
                this.showCredAsstData = response.items;
              });
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  onSubmitCredential(): void {
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
      .post(`${CredentialsApiUrl.credentials}`, this.credentialForm.value, {
        observe: 'response',
      })
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
    if (!this.credentialForm.get('passwordSecret').touched)
      this.credentialForm.get('passwordSecret').setValue(null);
    else if (
      this.credentialForm.get('passwordSecret').touched &&
      !this.credentialForm.value.passwordSecret
    )
      this.credentialForm.get('passwordSecret').setValue('');
    const headers = this.helperService.getETagHeaders(this.eTag);
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
      .put(
        `${CredentialsApiUrl.credentials}/${this.currentUrlId}`,
        this.credentialForm.value,
        {
          observe: 'response',
          headers,
        }
      )
      .subscribe(
        (response) => {
          if (response) {
            this.httpService.success('Credential updated successfully');
            this.isSubmitted = false;
            this.credentialForm.reset();
            this.router.navigate(['/pages/credentials']);
          }
        },
        (error) => {
          if (error && error.error && error.error.status === 409) {
            this.isSubmitted = false;
            this.httpService.error(error.error.serviceErrors);
            this.getCredentialsById();
          }
        }
      );
  }

  getCredentialsById(): void {
    this.httpService
      .get(
        `${CredentialsApiUrl.credentials}/${CredentialsApiUrl.view}/${this.currentUrlId}`,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.body) {
          this.eTag = response.headers.get('etag');
          this.min = response.body.startDate;
          this.credentialForm.patchValue({ ...response.body });
        }
      });
  }
  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteCredential(ref): void {
    this.isDeleted = true;
    this.httpService
      .delete(`${CredentialsApiUrl.credentials}/${this.deleteId}`, {
        observe: 'response',
      })
      .subscribe(
        () => {
          ref.close();
          this.httpService.success('Deleted Successfully');
          this.isDeleted = false;
          this.httpService
            .get(
              `${CredentialsApiUrl.credentials}?$filter=name+eq+'${this.credentialForm.value.name}'and agentId+ne+null`
            )
            .subscribe((response) => {
              this.showCredAsstData = response.items;
              console.log(this.showCredAsstData);
            });
        },
        () => (this.isDeleted = false)
      );
  }
}

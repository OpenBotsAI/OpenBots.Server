import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { AgentApiUrl, AgentGroupAPiUrl } from '../../../webApiUrls';
import { Agents } from '../../../interfaces/agnets';
@Component({
  selector: 'ngx-agent-group',
  templateUrl: './agent-group.component.html',
  styleUrls: ['./agent-group.component.scss'],
})
export class AgentGroupComponent implements OnInit {
  agentGroupForm: FormGroup;
  isSubmitted = false;
  urlId: string;
  eTag: string;
  title = 'Add';
  allAgentsArr: any;
  // selectedItems: Array<any> = [];
  // dropdownList: Array<any> = [];
  // dropdownSettings: IDropdownSettings = {};
  settings = {};
  data = [];
  dropdownList = [];
  selectedItems = [];
  dropdownSettings = {};
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.getAllAgents();
    // this.multiDropDownSettings();
    if (this.urlId) {
      this.title = 'Update';
      this.getAgentGroupById();
    }
    this.agentGroupForm = this.initializeAgentGroupForm();
    this.dropdownList = [
      { id: 1, itemName: 'India' },
      { id: 2, itemName: 'Singapore' },
      { id: 3, itemName: 'Australia' },
      { id: 4, itemName: 'Canada' },
      { id: 5, itemName: 'South Korea' },
      { id: 6, itemName: 'Germany' },
      { id: 7, itemName: 'France' },
      { id: 8, itemName: 'Russia' },
      { id: 9, itemName: 'Italy' },
      { id: 10, itemName: 'Sweden' },
    ];
    // this.selectedItems = [
    //   { id: 2, itemName: 'Singapore' },
    //   { id: 3, itemName: 'Australia' },
    //   { id: 4, itemName: 'Canada' },
    //   { id: 5, itemName: 'South Korea' },
    // ];
    this.dropdownSettings = {
      // agentId: 'agentId',
      // agentName: 'agentName',
      // singleSelection: false,
      text: 'Select Agents',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      // enableSearchFilter: true,
      class: 'myclass custom-class',
    };
  }

  multiDropDownSettings(): void {
    this.settings = {
      singleSelection: false,
      idField: 'item_id',
      textField: 'item_text',
      enableCheckAll: true,
      selectAllText: 'Chọn All',
      unSelectAllText: 'Hủy chọn',
      allowSearchFilter: true,
      limitSelection: -1,
      clearSearchFilter: true,
      maxHeight: 197,
      itemsShowLimit: 3,
      searchPlaceholderText: 'Tìm kiếm',
      noDataAvailablePlaceholderText: 'Không có dữ liệu',
      closeDropDownOnSelection: false,
      showSelectedItemsAtTop: false,
      defaultOpen: false,
    };
  }

  initializeAgentGroupForm() {
    return this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      isEnabled: [''],
      selectedItems: [],
      description: [''],
    });
  }

  get controls() {
    return this.agentGroupForm.controls;
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.urlId) this.updateAgentGroup();
    else this.addAgentGroup();
  }

  updateAgentGroup() {
    const headers = this.helperService.getETagHeaders(this.eTag);
    this.httpService
      .put(`${AgentGroupAPiUrl.agentGroups}`, this.agentGroupForm.value, {
        observe: 'response',
        headers,
      })
      .subscribe(
        (response) => {
          if (response) {
            this.httpService.success('Agent goup created successfully');
            this.isSubmitted = false;
            this.agentGroupForm.reset();
            this.router.navigate(['/pages/agentgroup/list']);
          }
        },
        (error) => {
          if (error && error.error && error.error.status === 409) {
            this.isSubmitted = false;
            this.httpService.error(error.error.serviceErrors);
            this.getAgentGroupById();
          }
        }
      );
  }

  addAgentGroup() {
    this.httpService
      .post(`${AgentGroupAPiUrl.agentGroups}`, this.agentGroupForm.value)
      .subscribe((response) => {
        if (response) {
          this.isSubmitted = false;
          this.httpService.success('Agent goup created successfully');
          this.router.navigate(['/pages/agentgroup/list']);
        }
      });
  }

  getAgentGroupById(): void {
    this.httpService
      .get(`${AgentGroupAPiUrl.agentGroups}/${this.urlId}`, {
        observe: 'response',
      })
      .subscribe((response) => {
        console.log('response', response);
        if (response && response.body) {
          this.eTag = response.headers.get('etag');
          this.agentGroupForm.patchValue({ ...response.body });
        }
      });
  }

  getAllAgents(): void {
    this.httpService
      .get(`${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`)
      .subscribe((response) => {
        if (response) {
          // for (let data of response) {
          //   this.allAgentsArr = data.agentName;
          //   console.log('agents', this.allAgentsArr);
          // }

          for (let data of response) {
            data.id = data.agentId;
            data.itemName = data.agentName;
          }
          this.allAgentsArr = response;
          // this.allAgentsArr = this.allAgentsArr.map((x) => {
          //   (x.id = x.agentId), (x.itemName = x.agentName);
          // });
          console.log('agents', this.allAgentsArr);
          // this.agentGroupForm
          //   .get('selectedItems')
          //   .patchValue(this.allAgentsArr);
        }
      });
  }

  onItemSelect(item: any) {
    console.log(item);
  }
  onSelectAll(items: any) {
    console.log(items);
  }

  public onFilterChange(item: any) {
    console.log(item);
  }
  public onDropDownClose(item: any) {
    console.log(item);
  }

  public onDeSelect(item: any) {
    console.log(item);
  }

  public onDeSelectAll(items: any) {
    console.log(items);
  }
  OnItemDeSelect(item: any) {
    console.log(item);
    // console.log(this.selectedItems);
  }
}

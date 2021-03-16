import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllAgentGroupGridComponent } from './all-agent-group-grid.component';

describe('AllAgentGroupGridComponent', () => {
  let component: AllAgentGroupGridComponent;
  let fixture: ComponentFixture<AllAgentGroupGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllAgentGroupGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllAgentGroupGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

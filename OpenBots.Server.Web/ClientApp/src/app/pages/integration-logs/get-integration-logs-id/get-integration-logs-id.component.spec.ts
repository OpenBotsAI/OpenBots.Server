import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetIntegrationLogsIdComponent } from './get-integration-logs-id.component';

describe('GetIntegrationLogsIdComponent', () => {
  let component: GetIntegrationLogsIdComponent;
  let fixture: ComponentFixture<GetIntegrationLogsIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetIntegrationLogsIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetIntegrationLogsIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

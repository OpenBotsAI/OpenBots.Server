import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllEmailSettingComponent } from './all-email-setting.component';

describe('AllEmailSettingComponent', () => {
  let component: AllEmailSettingComponent;
  let fixture: ComponentFixture<AllEmailSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllEmailSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllEmailSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

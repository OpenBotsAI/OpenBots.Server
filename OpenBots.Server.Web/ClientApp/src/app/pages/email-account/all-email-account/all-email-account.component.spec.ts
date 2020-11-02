import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllEmailAccountComponent } from './all-email-account.component';

describe('AllEmailAccountComponent', () => {
  let component: AllEmailAccountComponent;
  let fixture: ComponentFixture<AllEmailAccountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllEmailAccountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllEmailAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

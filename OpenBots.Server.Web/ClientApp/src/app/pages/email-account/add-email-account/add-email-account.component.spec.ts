import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEmailAccountComponent } from './add-email-account.component';

describe('AddEmailAccountComponent', () => {
  let component: AddEmailAccountComponent;
  let fixture: ComponentFixture<AddEmailAccountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddEmailAccountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddEmailAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

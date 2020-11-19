import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetConfigValueComponent } from './get-config-value.component';

describe('GetConfigValueComponent', () => {
  let component: GetConfigValueComponent;
  let fixture: ComponentFixture<GetConfigValueComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetConfigValueComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetConfigValueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

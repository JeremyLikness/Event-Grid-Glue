import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule} from '@angular/common/http';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { BlobComponent } from './blob/blob.component';
import { BlobsComponent } from './blobs/blobs.component';

@NgModule({
  declarations: [
    AppComponent,
    BlobComponent,
    BlobsComponent
  ],
  imports: [
    HttpClientModule,
    BrowserModule
  ],
  providers: [
    { provide: "BaseUrl", useValue: "https://imagegrabber.azurewebsites.net/api/" }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

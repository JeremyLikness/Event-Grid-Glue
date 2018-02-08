import { Component, Inject, OnInit } from '@angular/core';
import { IBlobImage} from '../blobImage';
import { GetImagesService } from '../get-images.service';

@Component({
  selector: 'imgapp-blobs',
  templateUrl: './blobs.component.html',
  styleUrls: ['./blobs.component.css'],
  providers: [GetImagesService]
})
export class BlobsComponent implements OnInit {

  private api: string;
  public blobs: IBlobImage[] = [];

  constructor(private svc: GetImagesService) {
   }

  ngOnInit() {
    this.svc.getImages().subscribe(blobs => {
      this.blobs = [...blobs];
    });
  }

}
